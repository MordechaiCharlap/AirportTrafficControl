using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service
{
    public class FlightService : IFlightService
    {
        private readonly IRepository<Flight> _flightRepostory;
        public FlightService(IRepository<Flight> flightRepository)
        {
            _flightRepostory = flightRepository;
        }
        public async Task AddNewFlight(FlightDto flightDto)
        {
            Flight flight = new Flight()
            {
                IsAscending = flightDto.IsAscending,
                IsPending = true,
                IsDone = false
            };
            _flightRepostory.Create(flight);
            await _flightRepostory.SaveChangesAsync();
        }

        public IEnumerable<FlightDto> GetAll()
        {
            List<FlightDto> flights = new();

            _flightRepostory.GetAll().ToList().ForEach(flight =>
            {
                flights.Add(new FlightDto() { FlightId = flight.FlightId, IsAscending = flight.IsAscending });
            });
            return flights;
        }
    }
}
