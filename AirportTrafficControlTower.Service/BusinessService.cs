using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using AirportTrafficControlTower.Data.Contexts;
using AutoMapper;

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
        //public EventHandler ExitEvent;



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
            _routesCollection = _context.Routes.ToList();
            _stationsCollection = _context.Stations.ToList();
            //ExitEvent += (s, args) => SaveChanges();
        }

        public void StartApp()
        {
            //List<Station> allStations = await _stationService.GetAll();
            //foreach (Station station in allStations)
            //{
            //    if (station.OccupiedBy != null)
            //    {
            //        var flight = await _flightService.Get((int)station.OccupiedBy!);
            //        await StartTime(flight!);
            //    }
            //}
            //AppDomain.CurrentDomain.ProcessExit+=ExitEvent;
            List<Station> allStations = _stationsCollection.ToList();
            Parallel.ForEach(allStations, async station =>
            {
                if (station.OccupiedBy != null)
                {
                    var flight = _flightsCollection.First(flight => station.OccupiedBy == flight.FlightId);
                    await StartTime(flight!);
                }
            });
        }
        public async Task AddNewFlight(CreateFlightDto flightDto)
        {
            var newFlight = _mapper.Map<Flight>(flightDto);
            //await _flightService.Create(newFlight);
            //newFlight.IsPending = true;
            //newFlight.IsDone = false;
            //newFlight.SubmissionTime = DateTime.Now;
            SaveNewFlight(newFlight);
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
        private async Task MoveNextIfPossible(Flight flight)
        {
            Task task = null;
            Console.WriteLine($"Flight {flight.FlightId} is trying to move next");
            Station? nextStation = null;
            var currentStation = _stationsCollection.FirstOrDefault(station => station.OccupiedBy == flight.FlightId);
            int? currentStationNumber = currentStation != null ? currentStation.StationNumber : null;
            //var nextRoutes = new List<Route>();
            var nextRoutes = await _routeService.GetRoutesByCurrentStationAndAsc(currentStationNumber, flight.IsAscending);
            //_routesCollection.ToList().ForEach(route =>
            //{
            //    if (route.Source == currentStationNumber &&
            //    route.IsAscending == flight.IsAscending &&
            //    (route.DestinationStation == null || route.DestinationStation.OccupiedBy == null))
            //    {
            //        nextRoutes.Add(route);
            //    }

            //});
            var success = false;
            foreach (var route in nextRoutes)
            {
                if (!success)
                {
                    if (route.Destination == null)
                    {
                        success = true;
                        flight.IsDone = true;
                        UpdateFlight(flight);
                        Console.WriteLine($"Flight {flight.FlightId} is done");
                    }
                    else
                    {
                        //var station = _stationsCollection.First(station => station.StationNumber == route.Destination);
                        if (route.DestinationStation!.OccupiedBy == null)
                        {
                            success = true;
                            nextStation = _stationsCollection.First(station => station.StationNumber == (int)route.Destination!);
                            nextStation.OccupiedBy = flight.FlightId;
                            UpdateStation(nextStation);
                            Console.WriteLine($"Station {nextStation.StationNumber} occupation updated");
                        }
                    }
                }
            }
            if (success)
            {
                Console.WriteLine($"Flight {flight.FlightId} succeed");

                if (flight.IsPending)
                {
                    flight.IsPending = false;
                    UpdateFlight(flight);
                    Console.WriteLine($"Flight {flight.FlightId} started the route");
                }
                else
                {

                    LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = currentStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdatesCollection.Add(leavingUpdate);
                    SaveNewLiveUpdate(leavingUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} left station {currentStation!.StationNumber}");
                }
                if (!flight.IsDone)
                {
                    LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = nextStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdatesCollection.Add(enteringUpdate);
                    SaveNewLiveUpdate(enteringUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} enters station {nextStation!.StationNumber}, station {nextStation.StationNumber} is occupied by {nextStation.OccupiedBy}");
                    task = StartTime(flight);
                }
                else
                {
                    flight.TimerFinished = null;
                    UpdateFlight(flight);
                    Console.WriteLine($"Flight {flight.FlightId} finished the route");
                }
                if (currentStation != null)
                {
                    currentStation.OccupiedBy = null;
                    UpdateStation(currentStation);
                    await SendWaitingInLineFlightIfPossible(currentStation);
                }

            }
            else
                Console.WriteLine($"Flight {flight.FlightId} hasnt managed to move next");
            if (task != null) await task;

        }
        void SaveChanges()
        {
            lock (obj)
            {
                 _context.SaveChanges();
            }
        }
        private void SaveNewLiveUpdate(LiveUpdate update)
        {
            _context.LiveUpdates.Add(update);
            SaveChanges();

        }
        private void SaveNewFlight(Flight flight)
        {
            flight.IsPending = true;
            flight.IsDone = false;
            flight.SubmissionTime = DateTime.Now;
            _context.Flights.Add(flight);
            SaveChanges();
        }
        private void UpdateStation(Station station)
        {
            _context.Stations.Update(station);
            SaveChanges();

        }
        private void UpdateFlight(Flight flight)
        {
            _context.Flights.Update(flight);
            SaveChanges();
        }
        //private async Task MoveNextIfPossible(Flight flight)
        //{
        //    Task? timerTask = null;
        //    Console.WriteLine($"Flight {flight.FlightId} is trying to move next");
        //    Station? nextStation = null;
        //    var currentStation = await _stationService.GetStationByFlightId(flight.FlightId);
        //    var currentStation = _stationsCollection.FirstOrDefault(station => station.OccupiedBy == flight.FlightId);
        //    int? currentStationNumber = currentStation != null ? currentStation.StationNumber : null;
        //    var nextRoutes = await _routeService.GetRoutesByCurrentStationAndAsc(currentStationNumber, flight.IsAscending);
        //    var success = false;
        //    foreach (var route in nextRoutes)
        //    {
        //        if (!success)
        //        {
        //            if (route.DestinationStation == null)
        //            {
        //                success = true;
        //                flight.IsDone = true;
        //            }
        //            else if (route.DestinationStation.OccupiedBy == null)
        //            {
        //                success = true;
        //                nextStation = await _stationService.Get((int)route.Destination!);
        //                await _stationService.ChangeOccupyBy(nextStation!.StationNumber, flight.FlightId);
        //                Console.WriteLine($"Station {nextStation.StationNumber} occupation updated");
        //            }
        //        }
        //    }
        //    if (success)
        //    {
        //        Console.WriteLine($"Flight {flight.FlightId} succeed");

        //        if (flight.IsPending)
        //        {
        //            flight.IsPending = false;
        //            Console.WriteLine($"Flight {flight.FlightId} started the route");
        //        }
        //        else
        //        {

        //            LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = currentStation!.StationNumber, UpdateTime = DateTime.Now };
        //            await _liveUpdateService.Create(leavingUpdate);
        //            Console.WriteLine($"Flight {flight.FlightId} left station {currentStation!.StationNumber}");
        //        }
        //        if (!flight.IsDone)
        //        {
        //            LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = nextStation!.StationNumber, UpdateTime = DateTime.Now };
        //            await _liveUpdateService.Create(enteringUpdate);
        //            Console.WriteLine($"Flight {flight.FlightId} enters station {nextStation!.StationNumber}, station {nextStation.StationNumber} is occupied by {nextStation.OccupiedBy}");
        //            timerTask = Task.Run(async()=>await StartTime(flight));
        //        }
        //        else
        //        {
        //            flight.TimerFinished = null;
        //            Console.WriteLine($"Flight {flight.FlightId} finished the route");
        //        }
        //        if (currentStation != null)
        //        {
        //            await _stationService.ChangeOccupyBy(currentStation.StationNumber, null);
        //            await SendWaitingInLineFlightIfPossible(currentStation);
        //        }
        //        if (timerTask != null) await timerTask;

        //    }
        //    else
        //        Console.WriteLine($"Flight {flight.FlightId} hasnt managed to move next");

        //}
        //private async Task SendWaitingInLineFlightIfPossible(Station currentStation)
        //{
        //    //Find all the stations that point to the current empty station.
        //    var pointingStations = _routeService.GetPointingStations(currentStation);
        //    bool? isFirstAscendingStation = _routeService.IsFirstAscendingStation(currentStation);
        //    if (isFirstAscendingStation != null)
        //    {
        //        var selectedFlight = await _flightService.GetFirstFlightInQueue(pointingStations, (bool)isFirstAscendingStation!);
        //        if (selectedFlight != null) await MoveNextIfPossible(selectedFlight);
        //    }
        //}
        private async Task SendWaitingInLineFlightIfPossible(Station currentStation)
        {
            //Find all the stations that point to the current empty station.
            var pointingStations = new List<Station>();
            _routesCollection.ToList().ForEach(route =>
            {
                if (route.Destination == currentStation.StationNumber &&
                route.Source != null)
                {
                    pointingStations.Add(route.SourceStation!);
                }
            });
            bool? isFirstAscendingStation = null;
            var enteringRoute = _routesCollection.FirstOrDefault(route => route.Destination == currentStation.StationNumber && route.Source == null);
            if (enteringRoute != null) isFirstAscendingStation = enteringRoute.IsAscending;

            if (isFirstAscendingStation != null)
            {
                var selectedFlight = GetFirstFlightInQueue(pointingStations, (bool)isFirstAscendingStation!);
                if (selectedFlight != null) await MoveNextIfPossible(selectedFlight);
            }
        }
        private async Task StartTime(Flight flight)
        {
            flight.TimerFinished = false;
            UpdateFlight(flight);
            Console.WriteLine($"{flight.FlightId} timer started");
            var rand = new Random();
            await Task.Delay(rand.Next(3000, 10000));
            //Thread.Sleep(rand.Next(3000, 10000));
            Console.WriteLine($"{flight.FlightId} timer finished");
            flight.TimerFinished = true;
            UpdateFlight(flight);
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
            return selectedFlight;
        }
        //private async Task StartTime(Flight flight)
        //{
        //    flight.TimerFinished = false;
        //    Console.WriteLine($"{flight.FlightId} timer started");
        //    var rand = new Random();
        //    //await Task.Delay(rand.Next(3000, 10000));
        //    Thread.Sleep(rand.Next(3000, 10000));
        //    Console.WriteLine($"{flight.FlightId} timer finished");
        //    flight.TimerFinished = true;
        //    await MoveNextIfPossible(flight);
        //}
    }
}