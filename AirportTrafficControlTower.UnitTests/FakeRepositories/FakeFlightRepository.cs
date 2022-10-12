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
    public class FakeFlightRepository : IRepository<Flight>
    {
        public FakeFlightRepository()
        {

        }
        public FakeDbContext GetContext()
        {
            FakeDbContext context = new FakeDbContext();
            return context;
        }
        public void Create(Flight entity)
        {
            var _context = GetContext();
            _context.Flights.Add(entity);
            _context.SaveChanges();
        }
        public bool Delete(int id)
        {
            var flight = Get(id);
            if (flight == null) return false;
            else
            {
                var _context = GetContext();
                _context.Remove(flight);
                return true;
            }
        }

        public IQueryable<Flight> GetAll()
        {
            var _context = GetContext();
            return _context.Flights;
        }

        public Flight? Get(int id)
        {
            var _context = GetContext();
            return _context.Flights.Find(id);
        }

        public void SaveChanges()
        {
            var _context = GetContext();
            _context.SaveChanges();
        }
        public bool Update(Flight entity)
        {
            var _context = GetContext();
            var flight = Get(entity.FlightId);
            if (flight == null) return false;
            else
            {
                _context.Update(entity);
                _context.SaveChanges();
                return true;
            }
        }
    }
}
