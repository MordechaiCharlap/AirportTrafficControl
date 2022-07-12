using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service
{
    public class LiveUpdateService : ILiveUpdateService
    {
        private readonly IRepository<LiveUpdate> _liveUpdateRepository;
        public LiveUpdateService(IRepository<LiveUpdate> liveUpdateRepository)
        {
            _liveUpdateRepository = liveUpdateRepository;
        }
        public async Task Create(LiveUpdate entity)
        {
            _liveUpdateRepository.Create(entity);
            await _liveUpdateRepository.SaveChangesAsync();
        }

        public async Task<LiveUpdate?> Get(int id)
        {
            return await _liveUpdateRepository.GetById(id);
        }

        public async Task<List<LiveUpdate>> GetAll()
        {
            return await _liveUpdateRepository.GetAll().ToListAsync();
        }

        public Task<bool> Update(LiveUpdate entity)
        {
            throw new NotImplementedException();
        }
    }
}
