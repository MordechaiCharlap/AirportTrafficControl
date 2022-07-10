using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service.Interfaces
{
    public interface IBusinessService
    {
        Task<IEnumerable<int>> GetNextStations();
        Task<IEnumerable<FlightDto>> GetFinishedRoutesHistory();
        Task<IEnumerable<StationDto>> GetAllStationsStatus();
        Task<IEnumerable<FlightDto>> GetAllFlights();
        Task AddNewFlight(FlightDto flight);
    }
}
