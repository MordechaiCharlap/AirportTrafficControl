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
        List<Station> GetPointingStations(Station station);
        bool? IsFirstAscendingStation(Station currentStation);
    }
}
