using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using AirportTrafficControlTower.Data.Contexts;
using AutoMapper;
using Hangfire;

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
        List<Flight> _flightsCollection;
        List<LiveUpdate> _liveUpdatesCollection;
        List<Route> _routesCollection;
        List<Station> _stationsCollection;
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
        }

        public async Task StartApp()
        {
            List<Station> allStations = _stationsCollection;
            List<Task> allTasks = new();
            foreach (Station station in allStations)
            {
                if (station.OccupiedBy != null)
                {
                    var flight = _flightsCollection.First(flight => station.OccupiedBy == flight.FlightId);
                    var task = StartTime(flight);
                    allTasks.Add(task);
                }
            }
            var ascFirstFlight = _flightsCollection.FirstOrDefault(flight => flight.IsPending == true && flight.IsAscending);
            var descFirstFlight = _flightsCollection.FirstOrDefault(flight => flight.IsPending == true && !flight.IsAscending);
            if(ascFirstFlight!=null) allTasks.Add(MoveNextIfPossible(ascFirstFlight));
            if(descFirstFlight != null) allTasks.Add(MoveNextIfPossible(descFirstFlight));
            await Task.WhenAll(allTasks);
        }
        public async Task AddNewFlight(CreateFlightDto flightDto)
        {
            List<Task> list = new();
            for(int i = 0; i < 10; i++)
            {
                var newFlight = _mapper.Map<Flight>(flightDto);
                ContextFunctionsLock(2, newFlight);
                _flightsCollection.Add(newFlight);
                if (newFlight == _flightsCollection.First(flight => flight.IsPending == true && flight.IsAscending == newFlight.IsAscending))
                {
                    list.Add(MoveNextIfPossible(newFlight));
                }
            }
            await Task.WhenAll(list);
            //var newFlight = _mapper.Map<Flight>(flightDto);
            //ContextFunctionsLock(2, newFlight);
            //_flightsCollection.Add(newFlight);
            //var addFlight = BackgroundJob.Enqueue(() => MoveNextIfPossible(newFlight));
            //if (newFlight == _flightsCollection.First(flight => flight.IsPending == true && flight.IsAscending == newFlight.IsAscending))
            //await MoveNextIfPossible(newFlight);
            //Console.WriteLine($"asc = {newFlight.IsAscending} pend = {newFlight.IsPending} flightId = {newFlight.FlightId}");
        }

        public List<GetFlightDto> GetAllFlights()
        {
            var dtoFlightsList = new List<GetFlightDto>();
            _flightsCollection.ForEach(flight =>
            {
                var flightDto = _mapper.Map<GetFlightDto>(flight);
                dtoFlightsList.Add(flightDto);
            });
            return dtoFlightsList;

        }

        public async Task<List<GetStationDto>> GetAllStationsStatus()
        {
            List<Station> list = await _stationService.GetAll();
            List<GetStationDto> listDtos = new();
            list.ForEach(station =>
            {
                listDtos.Add(_mapper.Map<GetStationDto>(station));
            });
            return listDtos;
        }
        public List<Route> GetRoutesByCurrentStationAndAsc(int? currentStationNumber, bool isAscending)
        {
            var list2 = new List<Route>();
            _routesCollection.ForEach(route =>
            {
                if (route.Source == currentStationNumber && route.IsAscending == isAscending && (route.DestinationStation == null || route.DestinationStation.OccupiedBy == null))
                    list2.Add(route);
            });
            return list2;
        }
        public async Task MoveNextIfPossible(Flight flight)
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
                        ContextFunctionsLock(4, flight);
                        Console.WriteLine($"Flight {flight.FlightId} is done");
                    }
                    else
                    {
                        nextStation = GetStation((int)route.Destination!);
                        //nextStation = _stationsCollection.First(station => station.StationNumber == (int)route.Destination!);
                        Console.WriteLine($"Checking if station {nextStation.StationNumber} is empty");

                        if (nextStation.OccupiedBy == null)
                        {
                            Console.WriteLine($"{nextStation.StationNumber} is empty");

                            success = true;
                            nextStation.OccupiedBy = flight.FlightId;
                            ContextFunctionsLock(3, nextStation);
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
                    ContextFunctionsLock(4, flight);
                    Console.WriteLine($"Flight {flight.FlightId} started the route");
                }
                else
                {

                    LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = currentStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdatesCollection.Add(leavingUpdate);
                    ContextFunctionsLock(1, leavingUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} left station {currentStation!.StationNumber}");
                }
                if (!flight.IsDone)
                {
                    LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = nextStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdatesCollection.Add(enteringUpdate);
                    ContextFunctionsLock(1, enteringUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} enters station {nextStation!.StationNumber}, station {nextStation.StationNumber} is occupied by {nextStation.OccupiedBy}");
                    task = StartTime(flight);
                    //var startTimer = BackgroundJob.Enqueue(() => StartTime(flight));
                }
                else
                {
                    flight.TimerFinished = null;
                    ContextFunctionsLock(4, flight);
                    Console.WriteLine($"Flight {flight.FlightId} finished the route");
                }
                if (currentStation != null)
                {
                    currentStation.OccupiedBy = null;
                    ContextFunctionsLock(3, currentStation);
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
        private Station? GetStation(int number)
        {
            lock (obj)
            {
            return _context.Stations.Find(number);
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
            var sourcesStations = GetPointingStations(currentStation);
            bool? isFirstAscendingStation = IsFirstAscendingStation(currentStation);
            var selectedFlight = GetFirstFlightInQueue(sourcesStations, isFirstAscendingStation);
            if (selectedFlight != null)
            {
                Console.WriteLine($"Sending Flight {selectedFlight.FlightId} to try moving next (must work)");
                await MoveNextIfPossible(selectedFlight);
            }
        }
        private List<Station> GetPointingStations(Station station)
        {
            List<Station> pointingStations = new();
            _routesCollection.ForEach(route =>
            {
                if (route.Destination == station.StationNumber && route.Source != null)
                {
                    pointingStations.Add(route.SourceStation!);
                }
            });
            return pointingStations;
        }
        private bool? IsFirstAscendingStation(Station currentStation)
        {
            var waitingRoute = _routesCollection.FirstOrDefault(route => route.Destination == currentStation.StationNumber && route.Source == null);
            return waitingRoute == null ? null : waitingRoute.IsAscending;
        }

        public async Task StartTime(Flight flight)
        {
            flight.TimerFinished = false;
            ContextFunctionsLock(4, flight);
            Console.WriteLine($"{flight.FlightId} timer started");
            var rand = new Random();
            await Task.Delay(rand.Next(4000, 10000));
            Console.WriteLine($"{flight.FlightId} timer finished");
            flight.TimerFinished = true;
            ContextFunctionsLock(4, flight);
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
                Console.WriteLine("Trying to find a plane in the list to start the route");
                var pendingFirstFlight = _flightsCollection.FirstOrDefault(flight => flight.IsAscending == isFirstAscendingStation && flight.IsPending == true);
                if (pendingFirstFlight != null)
                {
                    Console.WriteLine("Found a flight in the list");
                    if (selectedFlight == null) selectedFlight = pendingFirstFlight;
                    else
                    {
                        if (selectedFlight.SubmissionTime >= pendingFirstFlight.SubmissionTime) selectedFlight = pendingFirstFlight;
                    }
                }
                else
                {
                    Console.WriteLine("Have Not Found a flight in the list");
                }
            }
            if (selectedFlight == null)
            {
                Console.WriteLine("No flight is waiting");
                return null;
            }
            else
            {
                Console.WriteLine($"{selectedFlight.FlightId} is the first line in queue");
                return selectedFlight;
            }
        }

        public async Task<List<GetFlightDto>> GetPendingFlightsByAsc(bool isAsc)
        {
            List<GetFlightDto> listDto = new();
            var list = await _context.Flights.Where(flight => flight.IsPending == true && flight.IsAscending == isAsc).ToListAsync();
            list.ForEach(flight =>
            {
                listDto.Add(_mapper.Map<GetFlightDto>(flight));
            });
            return listDto;
        }
    }
}