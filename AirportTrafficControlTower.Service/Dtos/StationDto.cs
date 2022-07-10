﻿using AirportTrafficControlTower.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service.Dtos
{
    public class StationDto
    {
        public int StationId { get; set; }
        public int? OccupiedBy { get; set; }
    }
}
