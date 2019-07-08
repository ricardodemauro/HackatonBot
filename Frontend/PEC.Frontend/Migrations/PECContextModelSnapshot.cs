﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PEC.Frontend.DataBase;

namespace PEC.Frontend.Migrations
{
    [DbContext(typeof(PECContext))]
    partial class PECContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity("PEC.Frontend.DataBase.DataObjects.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Brand");

                    b.Property<string>("Color");

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("Name");

                    b.Property<string>("Plate");

                    b.HasKey("Id");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("PEC.Frontend.DataBase.DataObjects.CarImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Base64")
                        .HasColumnType("BLOB");

                    b.Property<int>("CarId");

                    b.Property<DateTime>("CreationDate");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.ToTable("CarImages");
                });

            modelBuilder.Entity("PEC.Frontend.DataBase.DataObjects.CarImage", b =>
                {
                    b.HasOne("PEC.Frontend.DataBase.DataObjects.Car")
                        .WithMany("CarImages")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
