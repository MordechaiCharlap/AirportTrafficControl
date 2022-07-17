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
        Task<List<GetFlightDto>> GetFinishedRoutesHistory();
        Task<List<GetStationDto>> GetAllStationsStatus();
        List<GetFlightDto> GetAllFlights();
        Task AddNewFlight(CreateFlightDto flight);
        Task StartApp();
    }
}
