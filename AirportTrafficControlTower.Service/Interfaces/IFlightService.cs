using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using System;

namespace AirportTrafficControlTower.Service.Interfaces
{
    public interface IFlightService
    {
        Task AddNewFlight(FlightDto flightDto);
        IEnumerable<FlightDto> GetAll();
    }
}
