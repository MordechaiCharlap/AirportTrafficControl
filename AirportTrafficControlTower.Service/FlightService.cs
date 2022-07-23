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
        private readonly object _lock = new object();

        public FlightService(IRepository<Flight> flightRepository)
        {
            _flightRepostory = flightRepository;
        }

        public void Create(Flight flight)
        {
            flight.IsPending = true;
            flight.IsDone = false;
            flight.SubmissionTime = DateTime.Now;
            _flightRepostory.Create(flight);
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
            //All stations are already valid (occupied and by flights who are ascending/descending according to route)
            //The stations already including the Flight property in them (OccupyByNavigation)
                Flight? selectedFlight = null;
                foreach (var pointingStation in pointingStations)
                {
                    //Flight flightToCheck = _flightRepostory.
                    //    GetAll().
                    //    FirstOrDefault(flight => flight.FlightId == (int)flightId && flight.TimerFinished == true);
                    var flightToCheck = pointingStation.OccupiedByNavigation;
                    if (flightToCheck!.TimerFinished == true)
                    {
                        if (selectedFlight == null) selectedFlight = flightToCheck;
                        else
                        {
                            if (selectedFlight.Stations.FirstOrDefault(station => station.StationNumber == 3) == null)
                            {
                                if (selectedFlight.SubmissionTime >= flightToCheck!.SubmissionTime) selectedFlight = flightToCheck;
                            }
                            else
                            {
                                selectedFlight = flightToCheck;
                            }
                        }
                    }
                }
                //returns if its a first station in an ascendingRoute(true), descendingRoute(false) or neither(null)
                if (isFirstAscendingStation != null)
                {
                    Console.WriteLine("Trying to find a plane in the list to start the route");
                    var pendingFirstFlight = _flightRepostory.GetAll().
                        FirstOrDefault(flight => flight.IsAscending == isFirstAscendingStation &&
                                                 flight.IsPending == true &&
                                                 flight.TimerFinished == null);
                    if (pendingFirstFlight != null && selectedFlight == null)
                    {
                        Console.WriteLine("Found a flight in the list");
                        selectedFlight = pendingFirstFlight;
                        //So there wont be 6+7 that taking the same flight while one is proccessing

                    }
                    else
                    {
                        Console.WriteLine("Have Not Found a flight in the list");
                    }
                }
                if (selectedFlight == null)
                {
                    Console.WriteLine("No flight is waiting in pointing stations too (So no flight at all)");
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
            Flight flight = new();
            flight.FlightId = entity.FlightId;
            flight.SubmissionTime = entity.SubmissionTime;
            flight.IsAscending = entity.IsAscending;
            flight.IsDone = entity.IsDone;
            flight.IsPending = entity.IsPending;
            flight.TimerFinished = entity.TimerFinished;
            return _flightRepostory.Update(flight);
        }
    }
}
