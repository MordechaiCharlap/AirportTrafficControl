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
    public class FakeRouteRepository : IRepository<Route>
    {
        public FakeRouteRepository()
        {
        }
        private FakeDbContext GetContext()
        {
            FakeDbContext _context = new();
            return _context;
        }
        public void Create(Route entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Route> GetAll()
        {
            var _context = GetContext();
            return _context.Routes;
        }

        public Route? Get(int id)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            var _context = GetContext();
            _context.SaveChanges();
        }


        public bool Update(Route entity)
        {
            throw new NotImplementedException();
        }

    }
}
