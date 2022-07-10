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

        public async Task<bool> Delete(int id)
        {
            var station = await GetById(id);
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

        public async Task<Station?> GetById(int id)
        {
            return await _context.Stations.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Update(Station entity)
        {
            var station = await GetById(entity.StationId);
            if (station == null) return false;
            else
            {
                _context.Update(station);
                return true;
            }
        }
    }
}
