using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using AirportTrafficControlTower.Data.Contexts;
using AutoMapper;
using System.Windows.Threading;

namespace AirportTrafficControlTower.Service
{
    public class BusinessService : IBusinessService
    {
        private readonly IFlightService _flightService;
        private readonly IStationService _stationService;
        private readonly ILiveUpdateService _liveUpdateService;
        private readonly IRouteService _routeService;
        private readonly AirPortTrafficControlContext _context;
        private readonly IMapper _mapper;
        private readonly DispatcherTimer dispatcher;

        public BusinessService(IFlightService flightService, IStationService stationService, 
            ILiveUpdateService liveUpdateService, AirPortTrafficControlContext context, IMapper mapper, IRouteService routeService)
        {
            _flightService = flightService;
            _stationService = stationService;
            _liveUpdateService = liveUpdateService;
            _routeService = routeService;
            _context = context;
            _mapper = mapper;
        }

        public async Task AddNewFlight(CreateFlightDto flightDto)
        {
            var newFlight = _mapper.Map<Flight>(flightDto);
            await _flightService.Create(newFlight);
            MoveNextIfPossible(newFlight);
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

        public async Task<IEnumerable<int>> GetNextStations()
        {
            throw new NotImplementedException();
        }
        private async void MoveNextIfPossible(Flight flight)
        {
            var currentStation = _context.Stations.
                FirstOrDefault(station => station.OccupiedBy == flight.FlightId);
            int? stationNumber = currentStation != null ? currentStation.StationNumber : null;
            var nextRoutes = _context.Routes.
                Include(route => route.DestinationStation).
                Where(route => route.Source == stationNumber &&
                      route.IsAscending == flight.IsAscending &&
                      (route.DestinationStation == null || route.DestinationStation.OccupiedBy == null)).ToList();
            var success = false;
            nextRoutes.ForEach(route =>
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

                    }
                }
            });
            if (success)
            {
                if (flight.IsPending)
                {
                    flight.IsPending = false;
                }
                else
                {
                    LiveUpdate leavingUpdate = new() { FlightId = flight.FlightId, IsEntering = false, StationId = (int)stationNumber!, UpdateTime = DateTime.Now };
                    await _liveUpdateService.Create(leavingUpdate);
                    Console.WriteLine("Flight left station x");
                }
                if (!flight.IsDone)
                {
                    LiveUpdate enteringUpdate = new() { FlightId = flight.FlightId, IsEntering = true, StationId = (int)stationNumber!, UpdateTime = DateTime.Now };
                    await _liveUpdateService.Create(enteringUpdate);
                    Console.WriteLine("Flight enters station y");
                }
                else
                {
                    Console.WriteLine("Flight finished the route");
                }
                if (currentStation != null)
                {
                    currentStation!.OccupiedBy = null;
                    OccupyStationIfPossible(currentStation);
                }
                await _context.SaveChangesAsync();
            }

        }
        private async void OccupyStationIfPossible(Station currentStation)
        {
            //Find all the stations that point to the current empty station.
            var pointingStations = _routeService.GetPointingStations(currentStation);
            bool? isFirstAscendingStation = _routeService.IsFirstAscendingStation(currentStation);
            bool isAsc = (bool)isFirstAscendingStation;
            var selectedFlight = await _flightService.GetFirstFlightInQueue(pointingStations, isAsc);
            if (selectedFlight != null) MoveNextIfPossible(selectedFlight);


        }
        private async void StartTimer(Flight flight) {
        }
    }
}