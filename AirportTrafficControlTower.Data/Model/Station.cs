using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirportTrafficControlTower.Data.Model
{
    [Table("Station")]
    public partial class Station
    {
        public Station()
        {
            LiveUpdates = new HashSet<LiveUpdate>();
        }

        [Key]
        public int StationId { get; set; }
        public int? OccupiedBy { get; set; }

        [ForeignKey("OccupiedBy")]
        [InverseProperty("Stations")]
        public virtual Flight? OccupiedByNavigation { get; set; }
        [InverseProperty("Station")]
        public virtual ICollection<LiveUpdate> LiveUpdates { get; set; }
    }
}
