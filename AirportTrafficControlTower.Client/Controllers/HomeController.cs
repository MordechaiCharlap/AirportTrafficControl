using AirportTrafficControlTower.Client.Helper;
using AirportTrafficControlTower.Service.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AirportTrafficControlTower.Client.Controllers
{
    public class HomeController : Controller
    {
        AirportApi _api = new AirportApi();
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> GetPendingFlightsByAsc()
        {
            List<GetFlightDto> flightList = new();
            HttpClient client = _api.Initial();
            var isAsc = true;
            HttpResponseMessage res = await client.GetAsync("api/Airport/GetPendingFlightsByAsc");
            if (res.IsSuccessStatusCode)
            {
                var result = res.Content.ReadAsStringAsync().Result;
                flightList = JsonConvert.DeserializeObject<List<GetFlightDto>>(result);
            }

            //NoView
            //NoParameterPassed


            return View(flightList);


        }

    }
}