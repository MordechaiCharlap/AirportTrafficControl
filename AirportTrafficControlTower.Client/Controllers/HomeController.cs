using AirportTrafficControlTower.Client.Helper;
using AirportTrafficControlTower.Client.Models;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Dtos;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
namespace AirportTrafficControlTower.Client.Controllers
{
    public class HomeController : Controller
    {
        AirportApi _api = new AirportApi();
        bool isWorking = false;
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
        public IActionResult AddNewFlight(bool isAsc)
        {

            using (var client = _api.Initial())
            {
                CreateFlightDto newDto = new() { IsAscending = isAsc };
                string uri = "api/Airport/AddNewFlight";
                var newDtoJason = JsonConvert.SerializeObject(newDto);
                var payload = new StringContent(newDtoJason, Encoding.UTF8, "application/json");
                var res = client.PostAsync(uri, payload).Result.Content.ReadAsStringAsync().Result;
                return RedirectToAction("GetAllStationsStatus");
            }


        }
        public async Task<IActionResult> StartSimulator(int numOfFlights)
        {
            using (var client = _api.Initial())
            {
                SimulatorNumber simulatorNumber = new() { Number = numOfFlights };
                var simulatorNumberJson = JsonConvert.SerializeObject(simulatorNumber);
                string uri = "api/Airport/StartSimulator";
                var payload = new StringContent(simulatorNumberJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(uri, payload);
                await response.Content.ReadAsStringAsync();

                //var res = client.PostAsync(uri, payload).Result.Content.ReadAsStringAsync().Result;
                return RedirectToAction("GetAllStationsStatus");
            }
        }
        public async Task<IActionResult> SeeAllLiveUpdates(int pageNum = 1)
        {
            List<LiveUpdate> liveUpdates = new List<LiveUpdate>();
            using (HttpClient client = _api.Initial())
            {
                HttpResponseMessage res = await client.GetAsync("api/Airport/GetAllLiveUpdates");
                if (res.IsSuccessStatusCode)
                {
                    var result = res.Content.ReadAsStringAsync().Result;
                    liveUpdates = JsonConvert.DeserializeObject<List<LiveUpdate>>(result)!;
                }

            }

            liveUpdates.Reverse();
            var elementsCountForPage = 15;
            var startingIndex = (pageNum - 1) * elementsCountForPage;
            var count = Math.Min(elementsCountForPage, liveUpdates.Count - startingIndex);
            var pageList = liveUpdates.GetRange(startingIndex, count);
            var LastPageNum = liveUpdates.Count / elementsCountForPage;
            //checking if there is a small page in the end or there is no updates at all
            if (liveUpdates.Count % elementsCountForPage != 0||liveUpdates.Count==0)
            {
                LastPageNum++;
            }
            LiveUpdatesViewModel viewModel = new()
            {
                IsFirstPage = (pageNum == 1),
                IsLastPage = (pageNum == LastPageNum),
                LiveUpdatesList = pageList,
                CurrentPage = pageNum
            };
            return View(viewModel);




        }
        public async Task<JsonResult> LoadStations()
        {
            List<StationStatus> stationStatusList = new();
            using (HttpClient client = _api.Initial())
            {
                if (!isWorking)
                {
                    await client.GetAsync($"api/Airport/StartApp");
                    isWorking = true;
                }
                HttpResponseMessage res = await client.GetAsync($"api/Airport/GetStationsStatusList");
                if (res.IsSuccessStatusCode)
                {
                    var result = res.Content.ReadAsStringAsync().Result;
                    stationStatusList = JsonConvert.DeserializeObject<List<StationStatus>>(result)!;
                }
            }
            return Json(new { stationStatusList = stationStatusList });
        }
        public async Task<IActionResult> GetAllStationsStatus()
        {
            List<StationStatus> stationList = new();
            using (HttpClient client = _api.Initial())
            {
                HttpResponseMessage res = await client.GetAsync($"api/Airport/GetAllStationsStatus");
                if (res.IsSuccessStatusCode)
                {
                    var result = res.Content.ReadAsStringAsync().Result;
                    stationList = JsonConvert.DeserializeObject<List<StationStatus>>(result)!;
                }
            }
            return View(stationList);
        }
        public async Task<IActionResult> GetPendingFlightsByAsc(bool isAsc)
        {
            List<GetFlightDto> flightList = new();
            using (HttpClient client = _api.Initial())
            {
                HttpResponseMessage res = await client.GetAsync($"api/Airport/GetPendingFlightsByAsc/{isAsc}");
                if (res.IsSuccessStatusCode)
                {
                    var result = res.Content.ReadAsStringAsync().Result;
                    flightList = JsonConvert.DeserializeObject<List<GetFlightDto>>(result)!;
                }
            }

            if (isAsc)
                ViewData["Title"] = "Pending take-offs";
            else ViewData["Title"] = "Pending ascending flights";
            return View(flightList);
        }

    }
}