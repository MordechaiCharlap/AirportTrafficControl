﻿using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service
{
    public class RouteService : IRouteService
    {
        private readonly IRepository<Route> _routeRepository;
        public RouteService(IRepository<Route> routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task Create(Route entity)
        {
            _routeRepository.Create(entity);
            await _routeRepository.SaveChangesAsync();
        }

        public Route? Get(int id)
        {
            return _routeRepository.GetById(id);
        }

        public async Task<List<Route>> GetAll()
        {
            return await _routeRepository.GetAll().ToListAsync();
        }

        public List<Station> GetPointingStations(Station station)
        {
            List<Station> pointingStations = new();
            _routeRepository.GetAll().
                Include(route => route.SourceStation).
                Where(route => route.Destination == station.StationNumber &&
                route.Source != null).
                ToList().
                ForEach(route => pointingStations.Add(route.SourceStation!));
            return pointingStations;
        }

        public List<Route> GetRoutesByCurrentStationAndAsc(int? currentStationNumber, bool isAscending)
        {
            var list2 = _routeRepository.GetAll().
                Include(route => route.DestinationStation).
                Where(route => route.Source == currentStationNumber &&
                      route.IsAscending == isAscending &&
                      (route.DestinationStation == null || route.DestinationStation.OccupiedBy == null)).ToList();
            return list2;
        }

        public bool? IsFirstAscendingStation(Station currentStation)
        {
            var waitingRoute = _routeRepository.GetAll().
                FirstOrDefault(route => route.Destination == currentStation.StationNumber && route.Source == null);
            return waitingRoute == null ? null : waitingRoute.IsAscending;
        }

        public Task<bool> Update(Route entity)
        {
            throw new NotImplementedException();
        }
    }
}