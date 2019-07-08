using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PEC.Frontend.DataBase.DataObjects;

namespace PEC.Frontend.DataBase.Configurations
{
    public class CarImageConfiguration : IEntityTypeConfiguration<CarImage>
    {
        public void Configure(EntityTypeBuilder<CarImage> builder)
        {
            builder.ToTable("CarImages").HasKey(x => x.Id);
            builder.Property(x => x.Base64).HasColumnType("BLOB");
        }
    }
}
