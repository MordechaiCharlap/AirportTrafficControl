using AirportTrafficControlTower.Data.Model;

namespace AirportTrafficControlTower.Client.Models
{
    public class LiveUpdatesViewModel
    {
        public List<LiveUpdate> LiveUpdatesList { get; set; }
        public bool IsLastPage { get; set; }
        public bool IsFirstPage { get; set; }
        public int CurrentPage { get; set; }
    }
}
