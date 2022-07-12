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

        public BusinessService(IFlightService flightService, IStationService stationService,
            ILiveUpdateService liveUpdateService, AirPortTrafficControlContext context, IMapper mapper, IRouteService routeService)
        {
            _flightService = flightService;
            _stationService = stationService;
            _liveUpdateService = liveUpdateService;
            _routeService = routeService;
            _mapper = mapper;
        }
        public async Task StartApp()
        {
            List<Station> allStations = await _stationService.GetAll();
            Parallel.ForEach(allStations, async station =>
            {
                if (station.OccupiedBy != null)
                {
                    var flight = await _flightService.Get((int)station.OccupiedBy);

                    await StartTime(flight!);
                }
            });


        }
        public async Task AddNewFlight(CreateFlightDto flightDto)
        {
            var newFlight = _mapper.Map<Flight>(flightDto);
            await _flightService.Create(newFlight);
            await MoveNextIfPossible(newFlight);
            Console.WriteLine($"asc = {newFlight.IsAscending} pend = {newFlight.IsPending} flightId = {newFlight.FlightId}");
        }

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
            Console.WriteLine($"Flight {flight.FlightId} is trying to move next");
            Station? nextStation = null;
            var currentStation = await _stationService.GetStationByFlightId(flight.FlightId);
            int? currentStationNumber = currentStation != null ? currentStation.StationNumber : null;
            var nextRoutes = await _routeService.GetRoutesByCurrentStationAndAsc(currentStationNumber, flight.IsAscending);
            var success = false;
            foreach (var route in nextRoutes)
            {
                if (!success)
                {
                    if (route.DestinationStation == null)
                    {
                        success = true;
                        flight.IsDone = true;
                    }
                    else if (route.DestinationStation.OccupiedBy == null)
                    {
                        success = true;
                        nextStation = await _stationService.Get((int)route.Destination!);
                        await _stationService.ChangeOccupyBy(nextStation!.StationNumber, flight.FlightId);
                        Console.WriteLine($"Station {nextStation.StationNumber} occupation updated");
                    }
                }
            }
            if (success)
            {
                Console.WriteLine($"Flight {flight.FlightId} succeed");

                if (flight.IsPending)
                {
                    flight.IsPending = false;
                    Console.WriteLine($"Flight {flight.FlightId} started the route");
                }
                else
                {

                    LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = currentStation!.StationNumber, UpdateTime = DateTime.Now };
                    await _liveUpdateService.Create(leavingUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} left station {currentStation!.StationNumber}");
                }
                if (!flight.IsDone)
                {
                    LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = nextStation!.StationNumber, UpdateTime = DateTime.Now };
                    await _liveUpdateService.Create(enteringUpdate);
                    Console.WriteLine($"Flight {flight.FlightId} enters station {nextStation!.StationNumber}, station {nextStation.StationNumber} is occupied by {nextStation.OccupiedBy}");
                    Task timerTask = Task.Run(async() => await StartTime(flight));
                }
                else
                {
                    flight.TimerFinished = null;
                    Console.WriteLine($"Flight {flight.FlightId} finished the route");
                }
                if (currentStation != null)
                {
                    await _stationService.ChangeOccupyBy(currentStation.StationNumber, null);
                    await SendWaitingInLineFlightIfPossible(currentStation);
                }
                Console.WriteLine("Saved changes");

            }
            else
                Console.WriteLine($"Flight {flight.FlightId} hasnt managed to move next");

        }
        private async Task SendWaitingInLineFlightIfPossible(Station currentStation)
        {
            //Find all the stations that point to the current empty station.
            var pointingStations = _routeService.GetPointingStations(currentStation);
            bool? isFirstAscendingStation = _routeService.IsFirstAscendingStation(currentStation);
            var selectedFlight = await _flightService.GetFirstFlightInQueue(pointingStations, (bool)isFirstAscendingStation!);
            if (selectedFlight != null) await MoveNextIfPossible(selectedFlight);

        }
        private async Task StartTime(Flight flight)
        {
            flight.TimerFinished = false;
            Console.WriteLine($"{flight.FlightId} timer started");
            var rand = new Random();
            await Task.Delay(rand.Next(3000, 10000));
            Console.WriteLine($"{flight.FlightId} timer finished");
            flight.TimerFinished = true;
            await MoveNextIfPossible(flight);
        }
    }
}