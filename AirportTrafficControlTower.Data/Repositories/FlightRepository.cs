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
        private AirPortTrafficControlContext GetContext()
        {
            AirPortTrafficControlContext _context = new();
            return _context;
        }
        public void Create(Flight entity)
        {
            var _context = GetContext();
            _context.Add(entity);
        }
        public bool Delete(int id)
        {
            var flight = GetById(id);
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

        public Flight? GetById(int id)
        {
            var _context = GetContext();
            return _context.Flights.Find(id);
        }

        public void SaveChanges()
        {
            var _context = GetContext();
            _context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            var _context = GetContext();
            await _context.SaveChangesAsync();
        }

        public bool Update(Flight entity)
        {
            var _context = GetContext();
            var flight = GetById(entity.FlightId);
            if (flight == null) return false;
            else
            {
                _context.Update(entity);
                return true;
            }
        }
    }
}
