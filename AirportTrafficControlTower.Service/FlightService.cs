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

        public Flight? Get(int id)
        {
            return _flightRepostory.GetById(id);
        }

        public List<Flight> GetAll()
        {
            return _flightRepostory.GetAll().ToList();
        }
        public Flight? GetFirstFlightInQueue(List<Station> pointingStations, bool? isFirstAscendingStation)
        {
            Flight? selectedFlight = null;
            foreach (var pointingStation in pointingStations)
            {
                var flightId = pointingStation.OccupiedBy;
                if (flightId != null)
                {
                    Flight flightToCheck = _flightRepostory.GetAll().Include(flight => flight.Stations).First(flight => flight.FlightId == (int)flightId);
                    if (flightToCheck!.TimerFinished == true)
                    {
                        if (selectedFlight == null) selectedFlight = flightToCheck;
                        else
                        {
                            if (flightToCheck.Stations.FirstOrDefault(station => station.StationNumber == 3) != null)
                            {
                                if (selectedFlight.SubmissionTime >= flightToCheck!.SubmissionTime) selectedFlight = flightToCheck;
                            }
                                
                        }
                    }
                }
            }
            //returns if its a first station in an ascendingRoute(true), descendingRoute(false) or neither(null)

            if (isFirstAscendingStation != null)
            {
                Console.WriteLine("Trying to find a plane in the list to start the route");
                var pendingFirstFlight = _flightRepostory.GetAll().FirstOrDefault(flight => flight.IsAscending == isFirstAscendingStation && flight.IsPending == true);
                if (pendingFirstFlight != null)
                {
                    Console.WriteLine("Found a flight in the list");
                    if (selectedFlight == null) selectedFlight = pendingFirstFlight;
                    //else
                    //{
                        
                    //    if (selectedFlight.SubmissionTime >= pendingFirstFlight.SubmissionTime) selectedFlight = pendingFirstFlight;
                    //}
                }
                else
                {
                    Console.WriteLine("Have Not Found a flight in the list");
                }
            }
            if (selectedFlight == null)
            {
                Console.WriteLine("No flight is waiting");
                return null;
            }
            else
            {
                Console.WriteLine($"{selectedFlight.FlightId} is the first line in queue");
                return selectedFlight;
            }
        }

        public bool Update(Flight entity)
        {
            return _flightRepostory.Update(entity);
        }
    }
}
