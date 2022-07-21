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
        private object obj = new object();



        public BusinessService(IFlightService flightService, IStationService stationService,
            ILiveUpdateService liveUpdateService, IMapper mapper, IRouteService routeService)
        {
            _flightService = flightService;
            _stationService = stationService;
            _liveUpdateService = liveUpdateService;
            _routeService = routeService;
            _mapper = mapper;
        }

        public async Task StartApp()
        {
            List<Station> allStations = _stationService.GetAll();
            List<Task> allTasks = new();
            foreach (Station station in allStations)
            {
                if (station.OccupiedBy != null)
                {
                    var flight = _flightService.GetAll().First(flight => station.OccupiedBy == flight.FlightId);
                    var task = StartTime(flight);
                    allTasks.Add(task);
                }
            }
            var ascFirstFlight = _flightService.GetAll().FirstOrDefault(flight => flight.IsPending == true && flight.IsAscending);
            var descFirstFlight = _flightService.GetAll().FirstOrDefault(flight => flight.IsPending == true && !flight.IsAscending);
            if (ascFirstFlight != null) allTasks.Add(MoveNextIfPossible(ascFirstFlight));
            if (descFirstFlight != null) allTasks.Add(MoveNextIfPossible(descFirstFlight));
            await Task.WhenAll(allTasks);
        }
        //public async Task Simulator()
        //{
        //    List<Task> list = new();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        Flight newFlight = new() { IsAscending = i % 2 == 0 };
        //        ContextFunctionsLock(2, newFlight);
        //        _flightsCollection.Add(newFlight);
        //        if (newFlight == _flightsCollection.First(flight => flight.IsPending == true && flight.IsAscending == newFlight.IsAscending))
        //        {
        //            list.Add(MoveNextIfPossible(newFlight));
        //        }
        //    }
        //    await Task.WhenAll(list);
        //}
        public async Task AddNewFlight(CreateFlightDto flightDto)
        {
            var newFlight = _mapper.Map<Flight>(flightDto);
            _flightService.Create(newFlight);
            Task task = null;
            if (newFlight.FlightId == _flightService.GetAll().First(flight => flight.IsPending == true && flight.IsAscending == newFlight.IsAscending).FlightId)
            {
                task = MoveNextIfPossible(newFlight);
            }
            if (task != null) await task;
        }

        public List<GetFlightDto> GetAllFlights()
        {
            var dtoFlightsList = new List<GetFlightDto>();
            _flightService.GetAll().ForEach(flight =>
            {
                var flightDto = _mapper.Map<GetFlightDto>(flight);
                dtoFlightsList.Add(flightDto);
            });
            return dtoFlightsList;

        }

        public List<Station> GetAllStationsStatus()
        {
            return _stationService.GetAll();
        }
        public async Task MoveNextIfPossible(Flight flight)
        {
            Task task = null;
            Console.WriteLine($"Flight {flight.FlightId} is trying to move next");
            Station? nextStation = null;
            var currentStation = _stationService.GetAll().FirstOrDefault(station => station.OccupiedBy == flight.FlightId);
            int? currentStationNumber = currentStation?.StationNumber;
            var nextRoutes = _routeService.GetRoutesByCurrentStationAndAsc(currentStationNumber, flight.IsAscending);
            var success = false;
            if (!(_routeService.IsCircleOfDoom(nextRoutes)&&_stationService.CircleOfDoomIsFull()))
            {
                foreach (var route in nextRoutes)
                {
                    if (!success)
                    {
                        if (route.Destination == null)
                        {
                            success = true;
                            flight.IsDone = true;
                            _flightService.Update(flight);
                            Console.WriteLine($"success = Flight {flight.FlightId} is done");
                        }
                        else
                        {
                            nextStation = _stationService.GetAll().First(station => station.StationNumber == (int)route.Destination);
                            Console.WriteLine($"Checking if station {nextStation.StationNumber} is empty");

                            if (nextStation.OccupiedBy == null)
                            {
                                Console.WriteLine($"success = {nextStation.StationNumber} is empty");

                                success = true;
                                //_stationService.ChangeOccupyBy(nextStation.StationNumber, flight.FlightId);
                                ChangeOccupation(nextStation.StationNumber, flight.FlightId);
                                Console.WriteLine($"Station {nextStation.StationNumber} is now filled by {flight.FlightId}");
                            }
                            else
                                Console.WriteLine($"{nextStation.StationNumber} is not empty");
                        }

                    }
                }
            }
            else
            {
                Console.WriteLine("circle of doom");
            }
            
            if (success)
            {
                Console.WriteLine($"Flight {flight.FlightId} succeed");

                if (flight.IsPending)
                {
                    flight.IsPending = false;
                    _flightService.Update(flight);
                    Console.WriteLine($"Flight {flight.FlightId} started the route");
                }
                else
                {

                    LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = currentStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdateService.Create(leavingUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} left station {currentStation!.StationNumber}");
                }
                if (!flight.IsDone)
                {
                    LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = nextStation!.StationNumber, UpdateTime = DateTime.Now };
                    _liveUpdateService.Create(enteringUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} enters station {nextStation!.StationNumber}, station {nextStation.StationNumber} is occupied by {nextStation.OccupiedBy}");
                    task = StartTime(flight);
                }
                else
                {
                    flight.TimerFinished = null;
                    _flightService.Update(flight);
                    Console.WriteLine($"Flight {flight.FlightId} finished the route");
                }
                if (currentStation != null)
                {
                    //_stationService.ChangeOccupyBy(currentStation.StationNumber, null);
                    ChangeOccupation(currentStation.StationNumber, null);
                    Console.WriteLine($"{currentStation.StationNumber} is now available, trying to find new flight to get in");
                    await SendWaitingInLineFlightIfPossible(currentStation);
                }

            }
            else
                Console.WriteLine($"Flight {flight.FlightId} hasnt managed to move next");
            if (task != null) await task;

        }
        private void ChangeOccupation(int stationNumber, int? flightId)
        {
            lock (obj)
            {
            _stationService.ChangeOccupyBy(stationNumber, flightId);

            }
        }
        private void SaveNewLiveUpdate(LiveUpdate update)
        {
            using (var _context = new AirPortTrafficControlContext())
            {
                _context.LiveUpdates.Add(update);
                _context.SaveChanges();
            }


        }
        private void SaveNewFlight(Flight flight)
        {
            using (var _context = new AirPortTrafficControlContext())
            {
                flight.IsPending = true;
                flight.IsDone = false;
                flight.SubmissionTime = DateTime.Now;
                _context.Flights.Add(flight);
                _context.SaveChanges();
            }

        }
        private void UpdateStation(Station station)
        {
            using (var _context = new AirPortTrafficControlContext())
            {
                _context.Stations.Update(station);
                _context.SaveChanges();
            }


        }
        private void UpdateFlight(Flight flight)
        {
            using (var _context = new AirPortTrafficControlContext())
            {
                _context.Flights.Update(flight);
                _context.SaveChanges();
            }

        }

        private async Task SendWaitingInLineFlightIfPossible(Station currentStation)
        {
            var sourcesStations = GetPointingStations(currentStation);
            bool? isFirstAscendingStation = IsFirstAscendingStation(currentStation);
            Flight? selectedFlight;
            selectedFlight = _flightService.GetFirstFlightInQueue(sourcesStations, isFirstAscendingStation);
            if (selectedFlight != null)
            {
                Console.WriteLine($"Sending Flight {selectedFlight.FlightId} to try moving next (must work)");
                await MoveNextIfPossible(selectedFlight);
            }
        }
        private List<Station> GetPointingStations(Station station)
        {
            ;
            return _routeService.GetPointingStations(station);
        }
        private bool? IsFirstAscendingStation(Station currentStation)
        {
            var waitingRoute = _routeService.GetAll().FirstOrDefault(route => route.Destination == currentStation.StationNumber && route.Source == null);
            return waitingRoute == null ? null : waitingRoute.IsAscending;
        }

        public async Task StartTime(Flight flight)
        {
            flight.TimerFinished = false;
            _flightService.Update(flight);
            Console.WriteLine($"{flight.FlightId} timer started");
            var rand = new Random();
            await Task.Delay(rand.Next(500, 1500));
            Console.WriteLine($"{flight.FlightId} timer finished");
            flight.TimerFinished = true;
            _flightService.Update(flight);
            Console.WriteLine("Before Move Next function");
            await MoveNextIfPossible(flight);
        }

        

        public List<GetFlightDto> GetPendingFlightsByAsc(bool isAsc)
        {
            List<GetFlightDto> listDto = new();
            //ContextFunctionsLock(5, new Flight());

            var list = _flightService.GetAll().Where(flight => flight.IsPending == true && flight.IsAscending == isAsc).ToList();
            list.ForEach(flight =>
            {
                listDto.Add(_mapper.Map<GetFlightDto>(flight));
            });
            return listDto;
        }

        public List<LiveUpdate> GetAllLiveUpdates()
        {
            return _liveUpdateService.GetAll();
        }

        public List<StationStatus> GetStationsStatusList()
        {
            return _stationService.GetStationsStatusList();
        }
    }
}