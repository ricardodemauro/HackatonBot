using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PEC.Frontend.DataBase.Configurations;
using PEC.Frontend.DataBase.DataObjects;

namespace PEC.Frontend.DataBase
{
    public class PECContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarImage> CarImages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./DataBase/PEC.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CarConfiguration());
            modelBuilder.ApplyConfiguration(new CarImageConfiguration());
        }
    }
}
