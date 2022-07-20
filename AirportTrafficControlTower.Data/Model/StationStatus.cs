using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Data.Model
{
    public class StationStatus
    {
        public int StationNumber { get; set; }
        public int? FlightInStation { get; set; }
        public bool? IsAscending { get; set; }
    }
}
