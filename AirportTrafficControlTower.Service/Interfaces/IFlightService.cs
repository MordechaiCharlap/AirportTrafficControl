using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using System;

namespace AirportTrafficControlTower.Service.Interfaces
{
    public interface IFlightService:IService<Flight>
    {
        Task<List<Flight>> GetPendingFlightsByIsAscending(bool isAscending);
        Task<Flight?> GetFirstFlightInQueue(List<Station> pointingStations, bool? isFirstAscendingStation);
    }
}
