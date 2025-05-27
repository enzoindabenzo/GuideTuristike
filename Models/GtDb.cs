using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DK1.Models;
using System.Data.Entity;

namespace DK1.Models
{
    public class GtDb : DbContext
    {
        // Constructor: Uses the connection string from Web.config (DefaultConnection by default)
        public GtDb() : base("DefaultConnection")
        {
        }

        // DbSet for Destinations (will create Destinations table in your database)
        public DbSet<Destination> Destinations { get; set; }

        // You can add more DbSets here if you have other models

        public DbSet<Category> Categories { get; set; }
        
        public DbSet<Reservation> Reservations { get; set; }


    public System.Data.Entity.DbSet<DK1.Models.Cal.Location> Locations { get; set; }

    }
}