﻿using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace AirportTrafficControlTower.Manager.Controllers
{
    [EnableCors("myPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class AirportController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        public AirportController(IBusinessService businessService)
        {
            _businessService = businessService;

        }
        [Route("[action]", Name = "StartApp")]
        [HttpPost]
        public async Task StartApp()
        {
            await _businessService.StartApp();
        }
        [Route("[action]/{isAsc:bool}", Name = "GetPendingFlightsByAsc")]
        [HttpGet]
        public List<GetFlightDto> GetPendingFlightsByAsc(bool isAsc)
        {
            var list = _businessService.GetPendingFlightsByAsc(isAsc);
            return list;
        }
        [Route("[action]", Name = "GetAllStationsStatus")]
        [HttpGet]
        public List<Station> GetAllStationsStatus()
        {
            return _businessService.GetAllStationsStatus();

        }

        [Route("[action]", Name = "GetStationsStatusList")]
        [HttpGet]
        public List<StationStatus> GetStationsStatusList()
        {
            return _businessService.GetStationsStatusList();
        }

        [Route("[action]", Name = "SeeAllLiveUpdates")]
        [HttpGet]
        public List<LiveUpdate> GetAllLiveUpdates()
        {
            var list = _businessService.GetAllLiveUpdates();
            return list;
        }

        [Route("[action]", Name = "StartSimulator")]
        [HttpPost]
        public async Task StartSimulator(SimulatorNumber simNumber)
        {
            await _businessService.StartSimulator(simNumber.Number);
        }
        [Route("[action]", Name = "AddNewFlight")]
        [HttpPost]
        public async Task AddNewFlight(CreateFlightDto flight)
        {
            await _businessService.AddNewFlight(flight);
        }
    }
}