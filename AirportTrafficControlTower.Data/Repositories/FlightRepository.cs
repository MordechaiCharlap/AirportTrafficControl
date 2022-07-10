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
        public async Task<bool> Delete(int id)
        {
            var flight = await GetById(id);
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

        public async Task<Flight?> GetById(int id)
        {
            return await _context.Flights.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Update(Flight entity)
        {
            var flight = await GetById(entity.FlightId);
            if (flight == null) return false;
            else
            {
                _context.Update(flight);
                return true;
            }
        }
    }
}
