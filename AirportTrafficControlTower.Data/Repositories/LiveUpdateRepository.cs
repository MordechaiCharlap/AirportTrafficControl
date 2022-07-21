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

        public bool Delete(int id)
        {
            var update = GetById(id);
            if (update == null) return false;
            else
            {
                _context.Remove(update);
                return true;
            }
        }

        public IQueryable<LiveUpdate> GetAll()
        {
            var _context = new AirPortTrafficControlContext();
            return _context.LiveUpdates;
        }

        public LiveUpdate? GetById(int id)
        {
            return _context.LiveUpdates.Find(id);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public bool Update(LiveUpdate entity)
        {
            var update = GetById(entity.LiveUpdateId);
            if (update == null) return false;
            else
            {
                _context.Update(entity);
                return true;
            }
        }
    }
}
