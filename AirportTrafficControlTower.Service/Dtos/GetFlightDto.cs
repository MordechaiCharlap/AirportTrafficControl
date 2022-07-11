using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service.Dtos
{
    public class GetFlightDto
    {
        public int FlightId { get; set; }
        public bool IsAscending { get; set; }
        public bool IsPending { get; set; }
        public bool IsDone { get; set; }
        public DateTime SubmissionTime { get; set; }
    }
}
