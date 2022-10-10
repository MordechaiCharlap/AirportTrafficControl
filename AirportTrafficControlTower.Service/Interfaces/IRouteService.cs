using AirportTrafficControlTower.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service.Interfaces
{
    public interface IRouteService:IService<Route>
    {
        List<Route> GetPointingRoutes(Station station);
        bool IsFirstStation(Station currentStation, bool isAscending);
        List<Route> GetRoutesByCurrentStationAndAsc(int? currentStationNumber, bool isAscending);
        bool IsCircleOfDoom(List<Route> nextRoutes);
    }
}
