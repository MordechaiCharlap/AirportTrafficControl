using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using AirportTrafficControlTower.Data.Contexts;

namespace AirportTrafficControlTower.Service
{
    public class BusinessService : IBusinessService
    {
        private readonly IFlightService _flightService;
        private readonly IStationService _stationService;
        private readonly ILiveUpdateService _liveUpdateService;
        private readonly AirPortTrafficControlContext _context;

        public BusinessService(IFlightService flightService, IStationService stationService, ILiveUpdateService liveUpdateService, AirPortTrafficControlContext context)
        {
            _flightService = flightService;
            _stationService = stationService;
            _liveUpdateService = liveUpdateService;
            _context = context;
        }

        public async Task AddNewFlight(FlightDto flight)
        {
        }

        public async Task<IEnumerable<FlightDto>> GetAllFlights()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StationDto>> GetAllStationsStatus()
        {
            List<StationDto> listDtos = new();
            IEnumerable<StationDto> list = await _stationService.GetAll();
            return list;
        }

        public async Task<IEnumerable<FlightDto>> GetFinishedRoutesHistory()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<int>> GetNextStations()
        {
            throw new NotImplementedException();
        }
    }
}