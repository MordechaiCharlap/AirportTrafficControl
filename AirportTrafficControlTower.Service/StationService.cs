using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service
{
    
    public class StationService : IStationService
    {
        private readonly IRepository<Station> _stationRepository;
        public StationService(IRepository<Station> stationRepository)
        {
            _stationRepository = stationRepository;
        }
        public async Task<IEnumerable<StationDto>> GetAll()
        {
            List<StationDto> listDtos = new();
            var stationsList = await _stationRepository.GetAll().ToListAsync();
            _stationRepository.GetAll().ToList().ForEach(station =>
            {
                StationDto stationDto = new() { StationId = station.StationId, OccupiedBy = station.OccupiedBy };
                listDtos.Add(stationDto);
            });
            return listDtos;
        }
    }
}
