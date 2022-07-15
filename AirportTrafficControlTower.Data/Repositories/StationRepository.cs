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
        public void Create(Station entity)
        {
            _context.Add(entity);
        }

        public bool Delete(int id)
        {
            var station =GetById(id);
            if (station == null) return false;
            else
            {
                _context.Remove(station);
                return true;
            }
        }

        public IQueryable<Station> GetAll()
        {
            return _context.Stations;
        }

        public Station? GetById(int id)
        {
            return _context.Stations.Find(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public bool Update(Station entity)
        {
            var station = GetById(entity.StationNumber);
            if (station == null) return false;
            else
            {
                _context.Stations.Update(station);
                return true;
            }
        }
    }
}
