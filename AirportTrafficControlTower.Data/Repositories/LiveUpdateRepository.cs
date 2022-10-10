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
        private AirPortTrafficControlContext GetContext()
        {
            AirPortTrafficControlContext _context = new();
            return _context;
        }

        public void Create(LiveUpdate entity)
        {
            var _context = GetContext();
            _context.Add(entity);
            _context.SaveChanges();

        }

        public bool Delete(int id)
        {
            var update = Get(id);
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

        public LiveUpdate? Get(int id)
        {
            return _context.LiveUpdates.Find(id);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public bool Update(LiveUpdate entity)
        {

            var _context = GetContext();
            _context.LiveUpdates.Update(entity);
            _context.SaveChanges();
            return true;

        }

        public Task UpdateAsync(Station station)
        {
            throw new NotImplementedException();
        }
    }
}
