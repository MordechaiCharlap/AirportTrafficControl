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

        public void ChangeOccupyBy(int stationNumber, int? flightId)
        {
            Station station = new() { StationNumber = stationNumber, OccupiedBy = flightId };
            _stationRepository.Update(station);
        }

        public bool CircleOfDoomIsFull()
        {
            int count = 0;
            _stationRepository.GetAll().ToList().ForEach(station =>
            {
                if (station.StationNumber >= 4 && station.StationNumber <= 8 && station.OccupiedBy != null) count++;
            });
            if (count == 4) return true;
            return false;
        }

        public void Create(Station entity)
        {
            throw new NotImplementedException();
        }

        public Station? Get(int id)
        {
            return _stationRepository.GetById(id);
        }

        public List<Station> GetAll()
        {
            return _stationRepository.GetAll().ToList();
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
