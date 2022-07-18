using AirportTrafficControlTower.Client.Helper;
using AirportTrafficControlTower.Data.Model;
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
        public async Task<IActionResult> SeeAllLiveUpdates()
        {
            List<LiveUpdate> liveUpdates = new List<LiveUpdate>();
            HttpClient client = _api.Initial();
            HttpResponseMessage res = await client.GetAsync("api/Airport/GetAllLiveUpdates");
            if (res.IsSuccessStatusCode)
            {
                var result = res.Content.ReadAsStringAsync().Result;
                liveUpdates = JsonConvert.DeserializeObject<List<LiveUpdate>>(result)!;
            }
            return View(liveUpdates);
        }
        public async Task<IActionResult> GetAllStationsStatus()
        {
            List<Station> stationList = new();
            HttpClient client = _api.Initial();
            HttpResponseMessage res = await client.GetAsync($"api/Airport/GetAllStationsStatus");
            if (res.IsSuccessStatusCode)
            {
                var result = res.Content.ReadAsStringAsync().Result;
                stationList = JsonConvert.DeserializeObject<List<Station>>(result)!;
            }
            return View(stationList);
        }
        public async Task<IActionResult> GetPendingFlightsByAsc(bool isAsc)
        {
            List<GetFlightDto> flightList = new();
            HttpClient client = _api.Initial();
            HttpResponseMessage res = await client.GetAsync($"api/Airport/GetPendingFlightsByAsc/{isAsc}");
            if (res.IsSuccessStatusCode)
            {
                var result = res.Content.ReadAsStringAsync().Result;
                flightList = JsonConvert.DeserializeObject<List<GetFlightDto>>(result)!;
            }
            if (isAsc)
                ViewData["Title"] = "Pending take-offs";
            else ViewData["Title"] = "Pending ascending flights";
            return View(flightList);
        }

    }
}