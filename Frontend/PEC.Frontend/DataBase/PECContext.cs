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
            optionsBuilder.UseSqlServer("Data Source=tcp:hackadb.database.windows.net,1433;Initial Catalog=hackahack;User ID=radmin;Password=Brq.1234");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CarConfiguration());
            modelBuilder.ApplyConfiguration(new CarImageConfiguration());
        }
    }
}
