using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hangfire;
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
        [HttpGet]
        public async Task StartApp()
        {
            await  _businessService.StartApp();
            //var addFlight = BackgroundJob.Enqueue(()=>_businessService.StartApp());
        }
        [Route("[action]/{isAsc:bool}", Name = "GetPendingFlightsByAsc")]
        [HttpGet]
        public async Task<List<GetFlightDto>> GetPendingFlightsByAsc(bool isAsc)
        {
            var list = await _businessService.GetPendingFlightsByAsc(isAsc);
            return list;
        }

        [Route("[action]", Name = "GetAllFlights")]
        [HttpGet]
        public  List<GetFlightDto> GetAllFlights()
        {
            //###################################################
            //return null;
            return _businessService.GetAllFlights();
        }

        [Route("[action]", Name = "GetAllStationsStatus")]
        [HttpGet]
        public async Task<List<Station>> GetAllStationsStatus()
        {
            return await _businessService.GetAllStationsStatus();

        }

        [Route("[action]", Name = "GetStationsStatusList")]
        [HttpGet]
        public List<StationStatus> GetStationsStatusList()
        {
            return _businessService.GetStationsStatusList();
        }

        [Route("[action]", Name = "SeeAllLiveUpdates")]
        [HttpGet]
        public async Task<List<LiveUpdate>> GetAllLiveUpdates()
        {
            var list = await _businessService.GetAllLiveUpdates();
            return list;
        }

        [Route("[action]", Name = "AddNewFlightList")]
        [HttpPost]
        public async Task AddNewFlightList(int num, bool isAsc)
        {
            for (int i = 0; i < num; i++)
            {
                CreateFlightDto newFlight = new() { IsAscending = isAsc };
                AddNewFlight(newFlight);
                //_businessService.AddNewFlight(new() { IsAscending = isAsc });
            }
        }
        [Route("[action]", Name = "AddNewFlight")]
        [HttpPost]
        public async Task AddNewFlight(CreateFlightDto flight)
        {
            await _businessService.AddNewFlight(flight);
            //var addFlight = BackgroundJob.Enqueue(() => _businessService.AddNewFlight(flight));
        }
    }
}
