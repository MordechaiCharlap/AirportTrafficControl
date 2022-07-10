using AirportTrafficControlTower.Data.Contexts;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Data.Repositories
{
    public class LiveUpdateRepositry : IRepository<LiveUpdate>
    {
        private readonly AirPortTrafficControlContext _context;
        public LiveUpdateRepositry(AirPortTrafficControlContext context)
        {
            _context = context;
        }
        public void Create(LiveUpdate entity)
        {
            _context.Add(entity);
        }

        public async Task<bool> Delete(int id)
        {
            var update = await GetById(id);
            if (update == null) return false;
            else
            {
                _context.Remove(update);
                return true;
            }
        }

        public IQueryable<LiveUpdate> GetAll()
        {
            return _context.LiveUpdates;
        }

        public async Task<LiveUpdate?> GetById(int id)
        {
            return await _context.LiveUpdates.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Update(LiveUpdate entity)
        {
            var update = await GetById(entity.LiveUpdateId);
            if (update == null) return false;
            else
            {
                _context.Update(update);
                return true;
            }
        }
    }
}
