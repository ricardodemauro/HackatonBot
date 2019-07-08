using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PEC.Frontend.DataBase.DataObjects;

namespace PEC.Frontend.DataBase.Configurations
{
    public class CarConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder.ToTable(nameof(PECContext.Cars)).HasKey(x => x.Id);
            builder.Property(x => x.Base64Image).HasColumnType("BLOB");
        }
    }
}
