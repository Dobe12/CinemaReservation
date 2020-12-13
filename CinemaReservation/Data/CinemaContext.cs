using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaReservation.Data
{
    public sealed class CinemaContext : DbContext
    {
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Seat> Seats { get; set; }

        public CinemaContext(DbContextOptions<CinemaContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cinema>().HasData(new List<Cinema>
            {
                new Cinema
                {
                    Id = 1,
                    Name = "CinemaPark",
                    Address = "Садовая 12",
                }
            });
            modelBuilder.Entity<Hall>().HasData(new List<Hall>
            {
                new Hall
                {
                    Id = 1,
                    Number = 8,
                    CinemaId = 1 
                }
            });
            modelBuilder.Entity<Seat>().HasData(new List<Seat>
            {
                new Seat
                {
                    Id = 1,
                    IsReserved = false,
                    Place = 10,
                    Row = 5,
                    HallId = 1
                },
                new Seat
                {
                    Id = 2,
                    IsReserved = true,
                    Place = 10,
                    Row = 5,
                    HallId = 1,
                    PlaceHolderPhone = 89241278006
                },

            });
        }
    }
}
