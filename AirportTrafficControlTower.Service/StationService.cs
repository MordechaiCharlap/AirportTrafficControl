﻿using AirportTrafficControlTower.Data.Model;
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
        public bool CircleOfDoomIsFull()
        {
            int count = 0;
            _stationRepository.GetAll().ToList().ForEach(station =>
            {
                if (station.StationNumber >= 4 && station.StationNumber <= 8 && station.OccupiedBy != null) count++;
            });
            if (count >= 4) return true;
            return false;
        }

        public void Create(Station entity)
        {
            throw new NotImplementedException();
        }

        public Station? Get(int id)
        {
            return _stationRepository.Get(id);
        }

        public List<Station> GetAll()
        {
            return _stationRepository.GetAll().ToList();
        }

        public List<Station> GetOccupiedPointingStations(List<Route> pointingRoutes)
        {
            List<Station> validPointingStations = new();

            List<Station> allStations = _stationRepository.
                GetAll().
                Include(station => station.OccupiedByNavigation).
                ToList();
            pointingRoutes.ForEach(route =>
            {
                var station = allStations.Find(station => station.StationNumber == route.Source);
                var isAsc = route.IsAscending;
                if (station!.OccupiedBy != null && station.OccupiedByNavigation!.IsAscending == isAsc)
                    validPointingStations.Add(station);
            });
            return validPointingStations;
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
                        IsAscending = isAsc
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
