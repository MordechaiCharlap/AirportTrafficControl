using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service
{

    public class StationService : IStationService
    {
        private readonly IRepository<Station> _stationRepository;
        public StationService(IRepository<Station> stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task ChangeOccupyBy(int stationNumber, int? flightId)
        {
            var station = _stationRepository.GetById(stationNumber);
            station!.OccupiedBy=flightId;
            await _stationRepository.SaveChangesAsync();
        }

        public Task Create(Station entity)
        {
            throw new NotImplementedException();
        }

        public Station? Get(int id)
        {
            return _stationRepository.GetById(id);
        }

        public async Task<List<Station>> GetAll()
        {
            return await _stationRepository.GetAll().ToListAsync();
        }

        public async Task<Station?> GetStationByFlightId(int flightId)
        {
            return await _stationRepository.GetAll().
                FirstOrDefaultAsync(station => station.OccupiedBy == flightId);
        }

        public List<StationStatus> GetStationsStatusList()
        {
            List<StationStatus> list = new();
            _stationRepository.GetAll().Include(station => station.OccupiedByNavigation).
                ToList().
                ForEach(station =>
                {
                    bool? isAsc = station.OccupiedByNavigation != null ? station.OccupiedByNavigation.IsAscending : null;
                    list.Add(new StationStatus()
                    {
                        StationNumber = station.StationNumber,
                        FlightInStation = station.OccupiedBy,
                        IsAscending=isAsc
                    });
                });
            return list;
        }

        public bool Update(Station entity)
        {
                return _stationRepository.Update(entity);
        }
    }
}
