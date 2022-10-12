using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.UnitTests.FakeContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.UnitTests.FakeRepositories
{
    public class FakeLiveUpdateRepositry : IRepository<LiveUpdate>
    {
        public FakeLiveUpdateRepositry()
        {
        }
        private FakeDbContext GetContext()
        {
            FakeDbContext _context = new();
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
                var _context = GetContext();
                _context.Remove(update);
                return true;
            }
        }

        public IQueryable<LiveUpdate> GetAll()
        {
            var _context = GetContext();
            return _context.LiveUpdates;
        }

        public LiveUpdate? Get(int id)
        {
            var _context = GetContext();
            return _context.LiveUpdates.Find(id);
        }

        public void SaveChanges()
        {
            var _context = GetContext();
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
