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
        private object _lock1 = new object();



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
            if (ascFirstFlight != null)
            {
                allTasks.Add(MoveNextIfPossible(ascFirstFlight));
                //second Ascending beginning station
                ascFirstFlight = _flightService.
                    GetAll().
                    FirstOrDefault(flight => flight.IsPending == true &&
                                             flight.IsAscending &&
                                             flight.FlightId != ascFirstFlight.FlightId);
                if (ascFirstFlight != null) allTasks.Add(MoveNextIfPossible(ascFirstFlight));
            }
            if (descFirstFlight != null) allTasks.Add(MoveNextIfPossible(descFirstFlight));
            await Task.WhenAll(allTasks);
        }
        public async Task StartSimulator(int numOfFlights)
        {
            List<Task> list = new();
            for (int i = 0; i < numOfFlights; i++)
            {
                CreateFlightDto newFlight = new() { IsAscending = i % 2 == 0 };
                list.Add(AddNewFlight(newFlight));
            }
            await Task.WhenAll(list);
        }
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
        public async Task<bool> MoveNextIfPossible(Flight flight)
        {
            Task task1 = null;
            Task task2 = null;
            Console.WriteLine($"Flight {flight.FlightId} is trying to move next");
            Station? nextStation = null;
            var allOccupied = _stationService.GetAll().Where(station => station.OccupiedBy == flight.FlightId);
            if (allOccupied.Count() > 1)
            {
                throw new Exception("More than one station same flight");
            }
            var currentStation = _stationService.GetAll().FirstOrDefault(station => station.OccupiedBy == flight.FlightId);

            if (currentStation == null && !flight.IsPending)
            {
                throw new Exception($"Flight {flight.FlightId} that is not pending must be in a station");
            }

            int? currentStationNumber = currentStation?.StationNumber;
            var nextRoutes = _routeService.GetRoutesByCurrentStationAndAsc(currentStationNumber, flight.IsAscending);
            Console.WriteLine($"liht {flight.FlightId} getting next routes from {currentStationNumber}");
            var success = false;
            if (!(_routeService.IsCircleOfDoom(nextRoutes) && _stationService.CircleOfDoomIsFull()))
            {
                foreach (var route in nextRoutes)
                {
                    if (!success)
                    {
                        if (route.Destination == null)
                        {
                            success = true;
                            flight.IsDone = true;
                            flight.TimerFinished = null;
                            _flightService.Update(flight);
                            Console.WriteLine($"success = Flight {flight.FlightId} is done");
                            Console.WriteLine($"Flight {flight.FlightId} finished the route");
                            currentStation!.OccupiedBy = null;
                            _stationService.ChangeOccupyBy(currentStation.StationNumber, null);
                        }
                        else
                        {
                            lock (_lock1)
                            {
                                nextStation = _stationService.GetAll().First(station => station.StationNumber == (int)route.Destination);
                                Console.WriteLine($"Checking if station {nextStation.StationNumber} is empty");

                                if (nextStation.OccupiedBy == null)
                                {
                                    if (currentStation != null)
                                    {

                                        currentStation.OccupiedBy = null;
                                        _stationService.ChangeOccupyBy(currentStation.StationNumber, null);
                                        Console.WriteLine($"{currentStation.StationNumber} is now not occupied, curr.occupied: {_stationService.Get((int)currentStationNumber)!.OccupiedBy}");
                                    }
                                    Console.WriteLine($"success = {nextStation.StationNumber} is empty");

                                    success = true;

                                    flight.IsPending = false;
                                    flight.TimerFinished = false;
                                    _flightService.Update(flight);
                                    nextStation.OccupiedBy = flight.FlightId;
                                    _stationService.ChangeOccupyBy(nextStation.StationNumber, flight.FlightId);

                                    Console.WriteLine($"Station {nextStation.StationNumber} is now filled by {_stationService.Get(nextStation.StationNumber).OccupiedBy}");
                                }
                                else
                                    Console.WriteLine($"{nextStation.StationNumber} is not empty, its occupied by {nextStation.OccupiedBy}");
                            }

                        }

                    }

                }
                if (success)
                {
                    Console.WriteLine($"Flight {flight.FlightId} succeed");
                    _stationService.GetAll().ForEach(station =>
                    {
                        Console.WriteLine($"{station.StationNumber} - {station.OccupiedBy}");
                    });
                    if (currentStation != null)
                    {
                        LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = currentStation!.StationNumber, UpdateTime = DateTime.Now };
                        _liveUpdateService.Create(leavingUpdate);
                        Console.WriteLine($"Flight {flight.FlightId} left station {currentStation!.StationNumber}");
                        task1 = SendWaitingInLineFlightIfPossible(currentStation);

                    }
                    if (!flight.IsDone)
                    {
                        LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = nextStation!.StationNumber, UpdateTime = DateTime.Now };
                        _liveUpdateService.Create(enteringUpdate);
                        Console.WriteLine($"Flight {flight.FlightId} enters station {nextStation!.StationNumber}, station {nextStation.StationNumber} is occupied by {nextStation.OccupiedBy}");
                        task2 = StartTime(flight);

                        if (task1 != null) await task2;
                        if (task2 != null) await task2;
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine($"circle of doom************************************** flight {flight.FlightId} wont succeed");

                }
            }

            return false;
        }

        private async Task<bool> SendWaitingInLineFlightIfPossible(Station currentStation)
        {

            Flight? selectedFlight = null;
            lock (_lock1)
            {
                var pointingRoutes = _routeService.GetPointingRoutes(currentStation);
                var pointingStations = _stationService.GetOccupiedPointingStations(pointingRoutes);
                bool? isFirstAscendingStation = _routeService.IsFirstAscendingStation(currentStation);
                selectedFlight = _flightService.GetFirstFlightInQueue(pointingStations, isFirstAscendingStation);
            }

            if (selectedFlight != null)
            {
                Console.WriteLine($"Sending Flight {selectedFlight.FlightId} to try moving next (must work if not doom)");
                bool didSucceed = await MoveNextIfPossible(selectedFlight);
                if (didSucceed)
                    return true;
                Console.WriteLine($"couldnt pulled {selectedFlight.FlightId}");
            }
            return false;
        }

        public async Task StartTime(Flight flight)
        {

            Console.WriteLine($"{flight.FlightId} timer started");
            var rand = new Random();
            await Task.Delay(rand.Next(500, 500));
            Console.WriteLine($"{flight.FlightId} timer finished");

            Console.WriteLine("Before Move Next function");
            if (!await MoveNextIfPossible(flight))
            {
                flight.TimerFinished = true;
                _flightService.Update(flight);
            }

        }



        public List<GetFlightDto> GetPendingFlightsByAsc(bool isAsc)
        {
            List<GetFlightDto> listDto = new();

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