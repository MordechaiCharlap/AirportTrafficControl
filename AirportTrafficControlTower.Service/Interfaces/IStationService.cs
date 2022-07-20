using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service.Interfaces
{
    public interface IStationService : IService<Station>
    {
        Task<Station?> GetStationByFlightId(int flightId);
        Task ChangeOccupyBy(int stationNumber, int? flightId);
        List<StationStatus> GetStationsStatusList();
    }
}
