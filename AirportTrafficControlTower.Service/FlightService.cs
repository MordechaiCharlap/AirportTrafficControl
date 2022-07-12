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
        public async Task<Flight?> GetFirstFlightInQueue(List<Station> pointingStations, bool? isFirstAscendingStation)
        {
            Flight? selectedFlight = null;
            foreach (var pointingStation in pointingStations)
            {
                var flightId = pointingStation.OccupiedBy;
                if (flightId != null)
                {
                    Flight flightToCheck = await Get((int)flightId);
                    if (flightToCheck!.TimerFinished == true)
                    {
                        if (selectedFlight == null) selectedFlight = flightToCheck;
                        else
                        {
                            if (selectedFlight.SubmissionTime >= flightToCheck!.SubmissionTime) selectedFlight = flightToCheck;
                        }
                    }
                }
            }
            //returns if its a first station in an ascendingRoute(true), descendingRoute(false) or neither(null)

            if (isFirstAscendingStation != null)
            {

                var pendingFirstFlight = await GetFirstPendingByAsc((bool)isFirstAscendingStation);
                if (pendingFirstFlight!=null)
                {
                    if (selectedFlight == null) selectedFlight = pendingFirstFlight;
                    else
                    {
                        if (selectedFlight.SubmissionTime >= pendingFirstFlight.SubmissionTime) selectedFlight = pendingFirstFlight;
                    }
                }
            }
            return selectedFlight;
        }

        private Task<Flight?> GetFirstPendingByAsc(bool isAscending)
        {
            throw new NotImplementedException();
        }
    }
}
