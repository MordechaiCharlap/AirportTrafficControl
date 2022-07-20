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
        Task<List<Station>> GetAllStationsStatus();
        List<StationStatus> GetStationsStatusList();
        List<GetFlightDto> GetAllFlights();
        Task AddNewFlight(CreateFlightDto flight);
        Task StartApp();
        Task<List<GetFlightDto>> GetPendingFlightsByAsc(bool isAsc);
        Task<List<LiveUpdate>> GetAllLiveUpdates();
    }
}
