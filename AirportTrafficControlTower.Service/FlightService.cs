using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AirportTrafficControlTower.Service
{
    public class FlightService : IFlightService
    {
        private readonly IRepository<Flight> _flightRepostory;
        public FlightService(IRepository<Flight> flightRepository)
        {
            _flightRepostory = flightRepository;
        }
        
        public async Task Create(Flight flight)
        {
            flight.IsPending = true;
            flight.IsDone = false;
            flight.SubmissionTime = DateTime.Now;
            _flightRepostory.Create(flight);
            await _flightRepostory.SaveChangesAsync();
        }

        public async Task<Flight?> Get(int id)
        {
            return await _flightRepostory.GetById(id);
        }

        public async Task<List<Flight>> GetAll()
        {
            return await _flightRepostory.GetAll().ToListAsync();
        }
    }
}
