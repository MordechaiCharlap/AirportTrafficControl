using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirportTrafficControlTower.Data.Model
{
    [Table("Route")]
    public partial class Route
    {
        [Key]
        public int RouteId { get; set; }
        public int? Source { get; set; }
        public bool IsAscending { get; set; }
        public int? Destination { get; set; }

        [ForeignKey("Destination")]
        [InverseProperty("RouteDestinations")]
        public virtual Station? DestinationStation { get; set; }
        [ForeignKey("Source")]
        [InverseProperty("RouteSources")]
        public virtual Station? SourceStation { get; set; }
    }
}
