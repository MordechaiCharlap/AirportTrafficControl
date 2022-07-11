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
    public class RouteRepository : IRepository<Route>
    {
        private readonly AirPortTrafficControlContext _context;
        public RouteRepository(AirPortTrafficControlContext context)
        {
            _context = context;
        }
        public void Create(Route entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Route> GetAll()
        {
            return _context.Routes;
        }

        public Task<Route?> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(Route entity)
        {
            throw new NotImplementedException();
        }
    }
}
