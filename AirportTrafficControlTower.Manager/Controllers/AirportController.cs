using AirportTrafficControlTower.Service.Dtos;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirportTrafficControlTower.Manager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirportController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public AirportController(IBusinessService businessService)
        {
            _businessService = businessService;
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
        [Route("[action]", Name = "AddNewFlight")]
        [HttpPost]
        public async Task AddNewFlight(CreateFlightDto flight)
        {
            await _businessService.AddNewFlight(flight);
        }
    }
}
