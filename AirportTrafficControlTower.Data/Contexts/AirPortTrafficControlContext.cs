using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using AirportTrafficControlTower.Data.Model;

namespace AirportTrafficControlTower.Data.Contexts
{
    public partial class AirPortTrafficControlContext : DbContext
    {
        public AirPortTrafficControlContext()
        {
        }

        public AirPortTrafficControlContext(DbContextOptions<AirPortTrafficControlContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Flight> Flights { get; set; } = null!;
        public virtual DbSet<LiveUpdate> LiveUpdates { get; set; } = null!;
        public virtual DbSet<Station> Stations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-R1MKK08\\CHARLAPSQLSERVER;Initial Catalog=AirportTrafficControl;Integrated Security=True");
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
                    .HasConstraintName("FK__LiveUpdat__Fligh__778AC167");

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.LiveUpdates)
                    .HasForeignKey(d => d.StationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__LiveUpdat__Stati__787EE5A0");
            });

            modelBuilder.Entity<Station>(entity =>
            {
                entity.HasOne(d => d.OccupiedByNavigation)
                    .WithMany(p => p.Stations)
                    .HasForeignKey(d => d.OccupiedBy)
                    .HasConstraintName("FK__Station__Occupie__71D1E811");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
