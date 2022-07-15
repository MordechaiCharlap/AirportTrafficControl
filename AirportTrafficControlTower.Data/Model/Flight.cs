using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirportTrafficControlTower.Data.Model
{
    [Table("Flight")]
    public partial class Flight:IEntity
    {
        public Flight()
        {
            LiveUpdates = new HashSet<LiveUpdate>();
            Stations = new HashSet<Station>();
        }

        [Key]
        public int FlightId { get; set; }
        public bool IsAscending { get; set; }
        public bool IsPending { get; set; }
        public bool IsDone { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime SubmissionTime { get; set; }
        public bool? TimerFinished { get; set; }
        [InverseProperty("Flight")]
        public virtual ICollection<LiveUpdate> LiveUpdates { get; set; }
        [InverseProperty("OccupiedByNavigation")]
        public virtual ICollection<Station> Stations { get; set; }
    }
}
