using AirportTrafficControlTower.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.UnitTests.FakeContext
{
    public class FakeDbContext:DbContext
    {
        public FakeDbContext()
        {
        }

        public FakeDbContext(DbContextOptions<FakeDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Flight> Flights { get; set; } = null!;
        public virtual DbSet<LiveUpdate> LiveUpdates { get; set; } = null!;
        public virtual DbSet<Route> Routes { get; set; } = null!;
        public virtual DbSet<Station> Stations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-R1MKK08\\CHARLAPSQLSERVER;Initial Catalog=FakeAirportTrafficControl;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LiveUpdate>(entity =>
            {
                entity.HasOne(d => d.Flight)
                    .WithMany(p => p.LiveUpdates)
                    .HasForeignKey(d => d.FlightId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__LiveUpdat__Fligh__2CF2ADDF");

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.LiveUpdates)
                    .HasForeignKey(d => d.StationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__LiveUpdat__Stati__2DE6D218");
            });

            modelBuilder.Entity<Route>(entity =>
            {
                entity.HasOne(d => d.SourceStation)
                   .WithMany(p => p.RouteSources)
                 .HasForeignKey(d => d.Source)
                    .HasConstraintName("FK__Route__Source__31B762FC");

                entity.HasOne(d => d.DestinationStation)
                 .WithMany(p => p.RouteDestinations)

                   .HasForeignKey(d => d.Destination)
                    .HasConstraintName("FK__Route__Destinati__30C33EC3");
            });

            modelBuilder.Entity<Station>(entity =>
            {
                entity.HasKey(e => e.StationNumber)
                    .HasName("PK__Station__26EDF8CC23B62B71");

                entity.Property(e => e.StationNumber).ValueGeneratedNever();

                entity.HasOne(d => d.OccupiedByNavigation)
                    .WithMany(p => p.Stations)
                    .HasForeignKey(d => d.OccupiedBy)
                    .HasConstraintName("FK__Station__Occupie__2A164134");
            });
        }

    }
}
