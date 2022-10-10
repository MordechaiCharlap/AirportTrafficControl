using AirportTrafficControlTower.Data.Contexts;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Data.Repositories
{
    public class StationRepository : IRepository<Station>
    {
        private readonly AirPortTrafficControlContext _context;
        public StationRepository(AirPortTrafficControlContext context)
        {
            _context = context;
        }
        private AirPortTrafficControlContext GetContext()
        {
            AirPortTrafficControlContext _context = new();
            return _context;
        }
        public void Create(Station entity)
        {
            var _context = GetContext();
            _context.Add(entity);
        }

        public bool Delete(int id)
        {
            var station = Get(id);
            if (station == null) return false;
            else
            {
                var _context = GetContext();
                _context.Remove(station);
                return true;
            }
        }
        public IQueryable<Station> GetAll()
        {
            var _context = GetContext();
            return _context.Stations;

        }

        public Station? Get(int id)
        {
            var _context = GetContext();
            return _context.Stations.FirstOrDefault(station => station.StationNumber == id);
        }

        public bool Update(Station entity)
        {
            var _context = GetContext();
            _context.Stations.Update(entity);
            _context.SaveChanges();
            return true;

        }
        public void SaveChanges()
        {
            var _context = GetContext();
            _context.SaveChanges();
        }
    }
}
