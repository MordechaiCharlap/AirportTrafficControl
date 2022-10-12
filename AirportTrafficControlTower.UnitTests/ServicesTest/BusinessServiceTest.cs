using AirportTrafficControlTower.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Interfaces;
using AirportTrafficControlTower.UnitTests.FakeContext;
using AirportTrafficControlTower.UnitTests.FakeRepositories;
using AirportTrafficControlTower.Service;
using AutoMapper;
using AirportTrafficControlTower.Service.Dtos;

namespace AirportTrafficControlTower.UnitTests.ServicesTest
{
    [TestClass]
    public class BusinessServiceTest
    {
        FakeFlightRepository flightRepository = new();
        FakeStationRepository stationRepository = new();
        FakeRouteRepository routeRepository = new();
        FakeLiveUpdateRepositry liveUpdateRepository = new();
        FlightService flightService;
        RouteService routeService;
        StationService stationService;
        LiveUpdateService liveUpdateService;
        BusinessService businessService;
        IMapper mapper;
        public BusinessServiceTest()
        {
            flightService = new(flightRepository);
            stationService = new(stationRepository);
            routeService = new(routeRepository);
            liveUpdateService = new(liveUpdateRepository);
            businessService = new(flightService, stationService, liveUpdateService, routeService, mapper!);
        }
        [TestMethod]
        public async Task NewFlightDoesntStartIfFirstStationOccupied()
        {
            Flight flight2;
            // One flight in station 1 and more two stations that are empty
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                var flight1 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = true, SubmissionTime = DateTime.Now };
                flight2 = new Flight() { IsAscending = true, IsPending = true, IsDone = false, TimerFinished = false, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.Flights.Add(flight2);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = flight1.FlightId });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = null });
                context.SaveChanges();

                //ascending route from 1 to 2
                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = true, Destination = null });
                context.SaveChanges();


            }
            //Test: flight2 should be still pending after trying to start.
            using (var context = new FakeDbContext())
            {
                //var flight2 = flightService.Get(flight2ID);
                await businessService.MoveNextIfPossible(flight2!);
                Assert.IsTrue(flight2!.IsPending);
            }
        }
        [TestMethod]
        public async Task FlightCanMoveIfNextStationIsEmpty()
        {
            // One flight in station 1 and more two stations that are empty
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                var flight1 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = true, SubmissionTime = DateTime.Now };
                var flight2 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = false, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.Flights.Add(flight2);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = flight1.FlightId });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = null });
                context.SaveChanges();

                //ascending route from 1 to 2
                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = true, Destination = null });
                context.SaveChanges();


            }
            using (var context = new FakeDbContext())
            {
                var flightAtStation1ID = stationService.Get(1)!.OccupiedBy;
                var flightAtStation1 = flightService.Get((int)flightAtStation1ID!);
                Assert.IsTrue(await businessService.MoveNextIfPossible(flightAtStation1!));
            }
        }
        [TestMethod]
        public async Task FlightCantMoveUnlessNextStationIsEmpty()
        {
            // Two flights, one at station 1 and one in station 2, the first shouldnt be able to move one
            // until the station 2 is avaliable.
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                var flight1 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = true, SubmissionTime = DateTime.Now };
                var flight2 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = false, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.Flights.Add(flight2);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = flight1.FlightId });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = flight2.FlightId });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = null });
                context.SaveChanges();

                //ascending route from 1 to 2
                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = true, Destination = null });
                context.SaveChanges();


            }
            using (var context = new FakeDbContext())
            {
                var flightAtStation1ID = stationService.Get(1)!.OccupiedBy;
                var flightAtStation1 = flightService.Get((int)flightAtStation1ID!);
                Assert.IsFalse(await businessService.MoveNextIfPossible(flightAtStation1!));
            }
        }
        [TestMethod]
        public async Task LastStationIsAvailableAfterMovingNext()
        {
            // One flight in station 1 and more two stations that are empty
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                var flight1 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = true, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = flight1.FlightId });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = null });
                context.SaveChanges();

                //ascending route from 1 to 2
                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = true, Destination = null });
                context.SaveChanges();


            }
            //Test: station is empty after a flight is moving on 
            using (var context = new FakeDbContext())
            {
                var flight1ID = stationService.Get(1)!.OccupiedBy;
                var flight1 = flightService.Get((int)flight1ID!);
                await businessService.MoveNextIfPossible(flight1!);
                Assert.IsNull(stationService.Get(1)!.OccupiedBy);
            }
        }
        [TestMethod]
        public async Task AssertLastStationOfRouteIsEmptyAfterMovingNext()
        {
            // One flight in station 1 and more two stations that are empty
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                var flight1 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = true, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = flight1.FlightId });
                context.SaveChanges();

                //ascending route from 1 to 2
                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = true, Destination = null });
                context.SaveChanges();


            }
            //Test: last station of the map is empty after a flight is moving on 
            using (var context = new FakeDbContext())
            {
                var flight1ID = stationService.Get(3)!.OccupiedBy;
                var flight1 = flightService.Get((int)flight1ID!);
                await businessService.MoveNextIfPossible(flight1!);
                Assert.IsNull(stationService.Get(1)!.OccupiedBy);
            }
        }
        [TestMethod]
        public async Task FlightDoesntMoveNextAfterTimerFinishedIfNextStationOccupied()
        {
            Flight flight1;
            // flight2 in station 2 and flight1 is pending
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                flight1 = new Flight() { IsAscending = true, IsPending = true, IsDone = false, TimerFinished = null, SubmissionTime = DateTime.Now };
                var flight2 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = false, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.Flights.Add(flight2);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = flight2.FlightId });
                context.SaveChanges();

                //ascending route from 1 to 2
                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = true, Destination = null });
                context.SaveChanges();


            }
            //Test: flight1 would go in the route, but would get stack at station 1
            //after flight2, the flight1 timer would be finished but it should stay at station1
            using (var context = new FakeDbContext())
            {

                await businessService.MoveNextIfPossible(flight1);

                Assert.IsTrue(flight1.TimerFinished);
                Assert.IsTrue(stationService.Get(2)!.OccupiedBy == flight1.FlightId);
            }
        }
        [TestMethod]
        public async Task UseRoutesAccordingToAscProperty()
        {

            // two flights pending in the list, flight1 is ascending flight2 is descending
            //creating a splitting map,
            //asc\     /desc
            //    2   3
            // asc \ / desc
            //      1
            //  asc||desc
            Flight flight1;
            Flight flight2;
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                flight1 = new Flight() { IsAscending = true, IsPending = true, IsDone = false, TimerFinished = null, SubmissionTime = DateTime.Now };
                flight2 = new Flight() { IsAscending = false, IsPending = true, IsDone = false, TimerFinished = null, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.Flights.Add(flight2);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = null });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = null });
                context.SaveChanges();


                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = null, IsAscending = false, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = null });
                context.Routes.Add(new() { Source = 1, IsAscending = false, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = false, Destination = null });
                context.SaveChanges();


            }
            using (var context = new FakeDbContext())
            {
                await businessService.StartApp();
                Assert.IsNull(flightService.GetAll().FirstOrDefault(flight => flight.IsPending == true));

                Assert.IsNotNull(liveUpdateService.GetAll().FirstOrDefault(update => update.FlightId == flight1.FlightId && update.StationId == 2));
                Assert.IsNotNull(liveUpdateService.GetAll().FirstOrDefault(update => update.FlightId == flight2.FlightId && update.StationId == 3));

                Assert.IsNull(liveUpdateService.GetAll().FirstOrDefault(update => update.FlightId == flight2.FlightId && update.StationId == 2));
                Assert.IsNull(liveUpdateService.GetAll().FirstOrDefault(update => update.FlightId == flight1.FlightId && update.StationId == 3));
            }
        }
        [TestMethod]
        public async Task AscFlightDoesntMoveToAvailableDescStation()
        {

            // flight1(ascending) at station 1. flight2 at station2.
            //creating a splitting map,
            //asc\     /desc
            //    2   3
            // asc \ / desc
            //      1
            //  asc||desc
            Flight flight1;
            Flight flight2;
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                flight1 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = false, SubmissionTime = DateTime.Now };
                flight2 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = false, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.Flights.Add(flight2);
                context.SaveChanges();

                context.Stations.Add(new() { StationNumber = 1, OccupiedBy = flight1.FlightId });
                context.Stations.Add(new() { StationNumber = 2, OccupiedBy = flight2.FlightId });
                context.Stations.Add(new() { StationNumber = 3, OccupiedBy = null });
                context.SaveChanges();


                context.Routes.Add(new() { Source = null, IsAscending = true, Destination = 1 });
                context.Routes.Add(new() { Source = null, IsAscending = false, Destination = 1 });
                context.Routes.Add(new() { Source = 1, IsAscending = true, Destination = 2 });
                context.Routes.Add(new() { Source = 2, IsAscending = true, Destination = null });
                context.Routes.Add(new() { Source = 1, IsAscending = false, Destination = 3 });
                context.Routes.Add(new() { Source = 3, IsAscending = false, Destination = null });
                context.SaveChanges();


            }
            using (var context = new FakeDbContext())
            {
                Assert.IsFalse(await businessService.MoveNextIfPossible(flight1));

            }
        }
        [TestMethod]
        public async Task NewFlightIsAddedToRepository()
        {

            // flight1(ascending) at station 1. flight2 at station2.
            //creating a splitting map,
            //asc\     /desc
            //    2   3
            // asc \ / desc
            //      1
            //  asc||desc
            Flight flight1;
            Flight flight2;
            
            using (var context = new FakeDbContext())
            {
                context.Flights.RemoveRange(context.Flights);
                context.Stations.RemoveRange(context.Stations);
                context.Routes.RemoveRange(context.Routes);
                context.LiveUpdates.RemoveRange(context.LiveUpdates);
                context.SaveChanges();
                Assert.AreEqual(0, flightRepository.GetAll().ToList().Count);
                flight1 = new Flight() { IsAscending = true, IsPending = false, IsDone = false, TimerFinished = false, SubmissionTime = DateTime.Now };
                context.Flights.Add(flight1);
                context.SaveChanges();
                Assert.AreEqual(1, flightRepository.GetAll().ToList().Count);
            }
        }


    }
}
