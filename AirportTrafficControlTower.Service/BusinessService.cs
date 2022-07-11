using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using AirportTrafficControlTower.Data.Contexts;
using AutoMapper;
using Microsoft.AspNetCore.Components;

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
        private readonly Dispatcher dispatcher;

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
                LiveUpdate update = new() { FlightId = flight.FlightId, IsEntering = false, StationId = (int)stationNumber!, UpdateTime=DateTime.Now};
                await _liveUpdateService.Create(update);
                if (currentStation != null)
                {
                    currentStation.OccupiedBy = null;
                    OccupyStationIfPossible(currentStation);
                }
                    
                if(flight.IsPending) 
                    flight.IsPending = false;
                await _context.SaveChangesAsync();

            }

        }
        private async void OccupyStationIfPossible(Station currentStation)
        {
            Flight? selectedFlight = null;
            bool success;
            //Find all the stations that point to the current empty station.
            var pointingStations = _routeService.GetPointingStations(currentStation);

            foreach (var pointingStation in pointingStations)
            {
                var flightId = pointingStation.OccupiedBy;
                if (flightId!=null)
                {
                    Flight flightToCheck = await _flightService.Get((int)flightId);
                    if (selectedFlight == null) selectedFlight = flightToCheck;
                    else
                    {
                        if (selectedFlight.SubmissionTime >= flightToCheck!.SubmissionTime) selectedFlight = flightToCheck;
                    }
                }
            }
            //returns if its a first station in an ascendingRoute(true), descendingRoute(false) or neither(null)
            bool? isFirstAscendingStation = _routeService.IsFirstAscendingStation(currentStation);
            if (isFirstAscendingStation != null)
            {
                bool isAsc = (bool)isFirstAscendingStation;
                var pendingList = await _flightService.GetPendingFlightsByIsAscending(isAsc);
                if (pendingList.Count!=0)
                {
                    if (selectedFlight == null) selectedFlight = pendingList[0];
                    else
                    {
                        if (selectedFlight.SubmissionTime >= pendingList[0].SubmissionTime) selectedFlight = pendingList[0];
                    }
                }
            }
            if (selectedFlight != null) MoveNextIfPossible(selectedFlight);


        }
        private async void StartTimer(Flight flight) {
        }
    }
}