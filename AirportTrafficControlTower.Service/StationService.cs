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

        public Task Create(Station entity)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Station>> GetAll()
        {
            return await _stationRepository.GetAll().ToListAsync();
        }
    }
}
