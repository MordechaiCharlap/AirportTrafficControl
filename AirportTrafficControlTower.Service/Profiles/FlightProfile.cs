using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Service.Dtos;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service.Profiles
{
    public class FlightProfile : Profile
    {
        public FlightProfile()
        {
            CreateMap<CreateFlightDto, Flight>();
            CreateMap<Flight, GetFlightDto>();
        }
    }
}
