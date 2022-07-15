using AirportTrafficControlTower.Data.Contexts;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirportTrafficControlTower.Data.Repositories
{
    public class FlightRepository : IRepository<Flight>
    {
        private readonly AirPortTrafficControlContext _context;
        public FlightRepository(AirPortTrafficControlContext context)
        {
            _context = context;
        }
        public void Create(Flight entity)
        {
            _context.Add(entity);
        }
        public bool Delete(int id)
        {
            var flight = GetById(id);
            if (flight == null) return false;
            else
            {
                _context.Remove(flight);
                return true;
            }
        }

        public IQueryable<Flight> GetAll()
        {
            return _context.Flights;
        }

        public Flight? GetById(int id)
        {
            return _context.Flights.Find(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public bool Update(Flight entity)
        {
            var flight = GetById(entity.FlightId);
            if (flight == null) return false;
            else
            {
                _context.Update(flight);
                return true;
            }
        }
    }
}
