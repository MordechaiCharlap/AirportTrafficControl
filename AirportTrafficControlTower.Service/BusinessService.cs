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
            Task task = null;
            Console.WriteLine($"Flight {flight.FlightId} is trying to move next");
            Station? nextStation = null;
            var currentStation = _stationService.GetAll().FirstOrDefault(station => station.OccupiedBy == flight.FlightId);

            if (currentStation == null && !flight.IsPending)
            {
                throw new Exception("Flight that is not pending must be in a station");
            }

            int? currentStationNumber = currentStation?.StationNumber;
            var nextRoutes = _routeService.GetRoutesByCurrentStationAndAsc(currentStationNumber, flight.IsAscending);
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
                        }
                        else
                        {
                            nextStation = _stationService.GetAll().First(station => station.StationNumber == (int)route.Destination);
                            Console.WriteLine($"Checking if station {nextStation.StationNumber} is empty");
                            lock (obj)
                            {
                                if (nextStation.OccupiedBy == null)
                                {
                                    Console.WriteLine($"success = {nextStation.StationNumber} is empty");

                                    success = true;
                                    _stationService.ChangeOccupyBy(nextStation.StationNumber, flight.FlightId);
                                    Console.WriteLine($"Station {nextStation.StationNumber} is now filled by {flight.FlightId}");
                                }

                                else
                                    Console.WriteLine($"{nextStation.StationNumber} is not empty");
                            }

                        }

                    }
                }
            }
            else
            {
                Console.WriteLine($"circle of doom************************************** flight {flight.FlightId} wont succeed");
            }

            if (success)
            {
                Console.WriteLine($"Flight {flight.FlightId} succeed");


                if (flight.IsPending)
                {
                    flight.IsPending = false;
                    _flightService.Update(flight);
                    Console.WriteLine($"Flight {flight.FlightId} started the route");
                    Console.WriteLine($"Flight {flight.FlightId} is not pending anymore!");
                }
                else
                {
                    if (currentStation == null) Console.WriteLine($"Flight {flight.FlightId} should be in a station but isnt ***********************************************************************************************************************************************");
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
                if (currentStation != null)
                {
                    if (!await SendWaitingInLineFlightIfPossible(_stationService.Get(currentStation.StationNumber)!))
                    {
                        _stationService.ChangeOccupyBy(currentStation.StationNumber, null);
                        Console.WriteLine($"{currentStation.StationNumber} havent found waiting flight.. turning empty");
                    }
                }
                if (task != null) await task;
                return true;
            }
            else
            {
                Console.WriteLine($"Flight {flight.FlightId} hasnt managed to move next");
                return false;
            }

        }
        
        private async Task<bool> SendWaitingInLineFlightIfPossible(Station currentStation)
        {

            Flight? selectedFlight = null;
            lock (obj)
            {
                var pointingRoutes = _routeService.GetPointingRoutes(currentStation);
                var pointingStations = _stationService.GetOccupiedPointingStations(pointingRoutes);
                bool? isFirstAscendingStation = _routeService.IsFirstAscendingStation(currentStation);
                selectedFlight = _flightService.GetFirstFlightInQueue(pointingStations, isFirstAscendingStation);
            }

            if (selectedFlight != null)
            {
                Console.WriteLine($"Sending Flight {selectedFlight.FlightId} to try moving next (must work if not doom)");
                if(await MoveNextIfPossible(selectedFlight))
                return true;
                Console.WriteLine($"couldnt pulled {selectedFlight.FlightId}");
                return false;
            }
            return false;
        }

        public async Task StartTime(Flight flight)
        {
            flight.TimerFinished = false;
            _flightService.Update(flight);
            Console.WriteLine($"{flight.FlightId} timer started");
            var rand = new Random();
            await Task.Delay(rand.Next(500, 1500));
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