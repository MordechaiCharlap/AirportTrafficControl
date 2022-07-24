using AirportTrafficControlTower.Data.Model;
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

        public void Create(Route entity)
        {
            _routeRepository.Create(entity);
            _routeRepository.SaveChanges();
        }

        public Route? Get(int id)
        {
            return _routeRepository.GetById(id);
        }

        public List<Route> GetAll()
        {
            return _routeRepository.GetAll().ToList();
        }

        public List<Route> GetPointingRoutes(Station station)
        {
            //not including routes from outside
            return _routeRepository.GetAll().
                Where(route => route.Destination == station.StationNumber &&
                route.Source != null).
                ToList();
        }

        public List<Route> GetRoutesByCurrentStationAndAsc(int? currentStationNumber, bool isAscending)
        {
            var list2 = _routeRepository.GetAll().
                Where(route => route.Source == currentStationNumber &&
                      route.IsAscending == isAscending).ToList();
            return list2;
        }

        public bool IsCircleOfDoom(List<Route> nextRoutes)
        {
            if (nextRoutes.FirstOrDefault(route =>
            ((route.Destination == 6 || route.Destination == 7) && route.IsAscending) ||
             (route.Destination == 4 && !route.IsAscending)) == null)
                return false;
            return true;
        }

        public bool? IsFirstAscendingStation(Station currentStation)
        {
            var waitingRoute = _routeRepository.GetAll().
                FirstOrDefault(route => route.Destination == currentStation.StationNumber && route.Source == null);
            return waitingRoute == null ? null : waitingRoute.IsAscending;
        }

        public bool Update(Route entity)
        {
            throw new NotImplementedException();
        }
    }
}
