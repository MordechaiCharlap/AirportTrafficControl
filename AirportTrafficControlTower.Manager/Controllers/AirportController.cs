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
        private readonly IBackgroundJobClient backgroundJobClient;
        private bool _isWorking = false;
        public AirportController(IBusinessService businessService, IBackgroundJobClient backgroundJobClient)
        {
            _businessService = businessService;
            this.backgroundJobClient = backgroundJobClient;
        }
        [Route("[action]", Name = "StartApp")]
        [HttpPost]
        public async Task StartApp()
        {
            if (!_isWorking)
            {
                _isWorking = true;
                var addFlight = BackgroundJob.Enqueue(() => _businessService.StartApp());
                await _businessService.StartApp();
            }

        }

        [Route("[action]", Name = "GetAllFlights")]
        [HttpGet]
        public async Task<IEnumerable<GetFlightDto>> GetAllFlights()
        {
            //###################################################
            //return null;
            return await _businessService.GetAllFlights();
        }
        [Route("[action]", Name = "GetAllStationsStatus")]
        [HttpGet]
        public async Task<IEnumerable<GetStationDto>> GetAllStationsStatus()
        {
            return await _businessService.GetAllStationsStatus();
        }
        [Route("[action]", Name = "AddNewFlightList")]
        [HttpPost]
        public async Task AddNewFlightList(List<CreateFlightDto> flights)
        {
            List<Task> tasks = new();
            foreach (var flight in flights)
            {
                var task = AddNewFlight(flight);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
        [Route("[action]", Name = "AddNewFlight")]
        [HttpPost]
        public async Task AddNewFlight(CreateFlightDto flight)
        {
            //HostingEnvironment.QueueBackgroundWorkItem(() => AddNewFlight(flight));
            var addFlight = BackgroundJob.Enqueue(() => _businessService.AddNewFlight(flight));
            //await _businessService.AddNewFlight(flight);
        }
    }
}
