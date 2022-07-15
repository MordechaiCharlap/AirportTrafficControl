using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using AirportTrafficControlTower.Data.Contexts;
using AutoMapper;
using Microsoft.AspNetCore.Hosting.Internal;

namespace AirportTrafficControlTower.Service
{
    public class BusinessService : IBusinessService
    {
        private readonly IFlightService _flightService;
        private readonly IStationService _stationService;
        private readonly ILiveUpdateService _liveUpdateService;
        private readonly IRouteService _routeService;
        private readonly IMapper _mapper;
        private readonly AirPortTrafficControlContext _context;
        ICollection<Flight> _flightsCollection;
        ICollection<LiveUpdate> _liveUpdatesCollection;
        ICollection<Route> _routesCollection;
        ICollection<Station> _stationsCollection;
        private object obj = new object();



        public BusinessService(IFlightService flightService, IStationService stationService,
            ILiveUpdateService liveUpdateService, IMapper mapper, IRouteService routeService, AirPortTrafficControlContext context)
        {
            _flightService = flightService;
            _stationService = stationService;
            _liveUpdateService = liveUpdateService;
            _routeService = routeService;
            _mapper = mapper;
            _context = context;
            _flightsCollection = _context.Flights.ToList();
            _liveUpdatesCollection = _context.LiveUpdates.ToList();
            _routesCollection = _context.Routes.Include(route => route.DestinationStation).Include(route => route.SourceStation).ToList();
            _stationsCollection = _context.Stations.ToList();
            //ExitEvent += (s, args) => SaveChanges();
        }

        public async Task StartApp()
        {
            List<Station> allStations = await _stationService.GetAll();
            List<Task> allTasks = new();
            foreach (Station station in allStations)
            {
                if (station.OccupiedBy != null)
                {
                    //var flight = await _flightService.Get((int)station.OccupiedBy!);
                    var flight = _flightsCollection.First(flight => station.OccupiedBy == flight.FlightId);
                    var task = StartTime(flight);
                    allTasks.Add(task);
                }
            }
            await Task.WhenAll(allTasks);
        }
        public async Task AddNewFlight(CreateFlightDto flightDto)
        {
            var newFlight = _mapper.Map<Flight>(flightDto);
            //await _flightService.Create(newFlight);
            //newFlight.IsPending = true;
            //newFlight.IsDone = false;
            //newFlight.SubmissionTime = DateTime.Now;
            ContextFunctionsLock(2,newFlight);
            _flightsCollection.Add(newFlight);
            await MoveNextIfPossible(newFlight);
            Console.WriteLine($"asc = {newFlight.IsAscending} pend = {newFlight.IsPending} flightId = {newFlight.FlightId}");
        }
        //public async Task AddNewFlight(CreateFlightDto flightDto)
        //{
        //    var newFlight = _mapper.Map<Flight>(flightDto);
        //    await _flightService.Create(newFlight);
        //    await MoveNextIfPossible(newFlight);
        //    Console.WriteLine($"asc = {newFlight.IsAscending} pend = {newFlight.IsPending} flightId = {newFlight.FlightId}");
        //}

        public async Task<IEnumerable<GetFlightDto>> GetAllFlights()
        {
            var flightsList = _flightService.GetAll();
            await Task.Delay(1000);
            return new List<GetFlightDto>();
            //flightDto mapper.Map<Flight>(GetFlightDto);

        }

        public async Task<IEnumerable<GetStationDto>> GetAllStationsStatus()
        {
            List<Station> list = await _stationService.GetAll();
            List<GetStationDto> listDtos = new();
            list.ForEach(station =>
            {
                listDtos.Add(_mapper.Map<GetStationDto>(station));
            });
            return listDtos;
        }

        public async Task<IEnumerable<GetFlightDto>> GetFinishedRoutesHistory()
        {
            throw new NotImplementedException();
        }
        public List<Route> GetRoutesByCurrentStationAndAsc(int? currentStationNumber, bool isAscending)
        {
            var list2 = new List<Route>();
            _routesCollection.ToList().ForEach(route =>
            {
                if (route.Source == currentStationNumber && route.IsAscending == isAscending && (route.DestinationStation == null || route.DestinationStation.OccupiedBy == null))
                    list2.Add(route);
            });
            return list2;
        }
        private async Task MoveNextIfPossible(Flight flight)
        {
            Task task = null;
            Console.WriteLine($"Flight {flight.FlightId} is trying to move next");
            Station? nextStation = null;
            var currentStation = _stationsCollection.FirstOrDefault(station => station.OccupiedBy == flight.FlightId);
            int? currentStationNumber = currentStation != null ? currentStation.StationNumber : null;
            var nextRoutes = GetRoutesByCurrentStationAndAsc(currentStationNumber, flight.IsAscending);
            var success = false;
            foreach (var route in nextRoutes)
            {
                if (!success)
                {
                    if (route.Destination == null)
                    {
                        success = true;
                        flight.IsDone = true;
                        ContextFunctionsLock(4,flight);
                        Console.WriteLine($"Flight {flight.FlightId} is done");
                    }
                    else
                    {
                        nextStation = _stationsCollection.First(station => station.StationNumber == (int)route.Destination!);
                        Console.WriteLine($"Checking if station {nextStation.StationNumber} is empty");
                        if (nextStation.OccupiedBy == null)
                        {
                            Console.WriteLine($"{nextStation.StationNumber} is empty");

                            success = true;
                            nextStation.OccupiedBy = flight.FlightId;
                            ContextFunctionsLock(3,nextStation);
                            Console.WriteLine($"Station {nextStation.StationNumber} is now filled by {flight.FlightId}");
                        }
                        else
                            Console.WriteLine($"{nextStation.StationNumber} is not empty");
                    }
                }
            }
            if (success)
            {
                Console.WriteLine($"Flight {flight.FlightId} succeed");

                if (flight.IsPending)
                {
                    flight.IsPending = false;
                    ContextFunctionsLock(4,flight);
                    Console.WriteLine($"Flight {flight.FlightId} started the route");
                }
                else
                {

                    LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = currentStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdatesCollection.Add(leavingUpdate);
                    ContextFunctionsLock(1,leavingUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} left station {currentStation!.StationNumber}");
                }
                if (!flight.IsDone)
                {
                    LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = nextStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdatesCollection.Add(enteringUpdate);
                    ContextFunctionsLock(1,enteringUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} enters station {nextStation!.StationNumber}, station {nextStation.StationNumber} is occupied by {nextStation.OccupiedBy}");

                    HostingEnvironment.QueueBackgroundWorkItem
                    task = StartTime(flight);
                }
                else
                {
                    flight.TimerFinished = null;
                    ContextFunctionsLock(4,flight);
                    Console.WriteLine($"Flight {flight.FlightId} finished the route");
                }
                if (currentStation != null)
                {
                    currentStation.OccupiedBy = null;
                    ContextFunctionsLock(3,currentStation);
                    Console.WriteLine($"{currentStation.StationNumber} is now available, trying to find new flight to get in");
                    await SendWaitingInLineFlightIfPossible(currentStation);
                }

            }
            else
                Console.WriteLine($"Flight {flight.FlightId} hasnt managed to move next");
            if (task != null) await task;

        }
        void ContextFunctionsLock(int num, IEntity entity)
        {
            lock (obj)
            {
                switch (num)
                {
                    case 1:
                        SaveNewLiveUpdate((LiveUpdate)entity);
                        break;
                    case 2:
                        SaveNewFlight((Flight)entity);
                        break;
                    case 3:
                        UpdateStation((Station)entity);
                        break;
                    case 4:
                        UpdateFlight((Flight)entity);
                        break;
                }
            }

        }
        private void SaveNewLiveUpdate(LiveUpdate update)
        {
            _context.LiveUpdates.Add(update);
            _context.SaveChanges();

        }
        private void SaveNewFlight(Flight flight)
        {
            flight.IsPending = true;
            flight.IsDone = false;
            flight.SubmissionTime = DateTime.Now;
            _context.Flights.Add(flight);
            _context.SaveChanges();
        }
        private void UpdateStation(Station station)
        {
            _context.Stations.Update(station);
            _context.SaveChanges();

        }
        private void UpdateFlight(Flight flight)
        {
            _context.Flights.Update(flight);
            _context.SaveChanges();
        }

        private async Task SendWaitingInLineFlightIfPossible(Station currentStation)
        {
            var sourcesStations = _routeService.GetPointingStations(currentStation);
            bool? isFirstAscendingStation = _routeService.IsFirstAscendingStation(currentStation);
            var selectedFlight = GetFirstFlightInQueue(sourcesStations, isFirstAscendingStation);
            if (selectedFlight != null)
            {
                Console.WriteLine($"Sending Flight {selectedFlight.FlightId} to try moving next (must work)");
                await MoveNextIfPossible(selectedFlight);
            }
        }


        private async Task StartTime(Flight flight)
        {
            flight.TimerFinished = false;
            ContextFunctionsLock(4,flight);
            Console.WriteLine($"{flight.FlightId} timer started");
            var rand = new Random();
            await Task.Delay(rand.Next(3000, 10000));
            //Thread.Sleep(rand.Next(3000, 10000));
            Console.WriteLine($"{flight.FlightId} timer finished");
            flight.TimerFinished = true;
            ContextFunctionsLock(4,flight);
            Console.WriteLine("Before Move Next function");
            await MoveNextIfPossible(flight);
        }

        private Flight? GetFirstFlightInQueue(List<Station> pointingStations, bool? isFirstAscendingStation)
        {
            Flight? selectedFlight = null;
            foreach (var pointingStation in pointingStations)
            {
                var flightId = pointingStation.OccupiedBy;
                if (flightId != null)
                {
                    Flight flightToCheck = _flightsCollection.First(flight => flight.FlightId == (int)flightId);
                    if (flightToCheck!.TimerFinished == true)
                    {
                        if (selectedFlight == null) selectedFlight = flightToCheck;
                        else
                        {
                            if (selectedFlight.SubmissionTime >= flightToCheck!.SubmissionTime) selectedFlight = flightToCheck;
                        }
                    }
                }
            }
            //returns if its a first station in an ascendingRoute(true), descendingRoute(false) or neither(null)

            if (isFirstAscendingStation != null)
            {
                var pendingFirstFlight = _flightsCollection.FirstOrDefault(flight => flight.IsAscending == isFirstAscendingStation && flight.IsPending == true);
                if (pendingFirstFlight != null)
                {
                    if (selectedFlight == null) selectedFlight = pendingFirstFlight;
                    else
                    {
                        if (selectedFlight.SubmissionTime >= pendingFirstFlight.SubmissionTime) selectedFlight = pendingFirstFlight;
                    }
                }
            }
            if (selectedFlight == null)
            {
                Console.WriteLine("No flight is waiting");
                return null;
            }
            else
            {
                Console.WriteLine($"{selectedFlight} is the first line in queue");
                return selectedFlight;
            }
        }


    }
}