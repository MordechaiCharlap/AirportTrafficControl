using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hangfire;

namespace AirportTrafficControlTower.Manager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirportController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private bool _isWorking = false;
        public AirportController(IBusinessService businessService)
        {
            _businessService = businessService;
        }
        [Route("[action]", Name = "StartApp")]
        [HttpPost]
        public void StartApp()
        {
            if (!_isWorking)
            {
                _isWorking = true;
                var startApp = BackgroundJob.Enqueue(() => _businessService.StartApp());
                //await _businessService.StartApp();
            }

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
        public async Task<IEnumerable<GetStationDto>> GetAllStationsStatus()
        {
            return await _businessService.GetAllStationsStatus();
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
