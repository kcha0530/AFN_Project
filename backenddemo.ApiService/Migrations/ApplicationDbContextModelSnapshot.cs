using System;
using backenddemo.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backenddemo.ApiService.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("backenddemo.ApiService.Models.User", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
                b.Property<string>("Email").IsRequired().HasMaxLength(255).HasColumnType("character varying(255)");
                b.Property<string>("FullName").IsRequired().HasColumnType("text");
                b.Property<bool>("IsActive").HasColumnType("boolean");
                b.Property<string>("PasswordHash").IsRequired().HasColumnType("text");
                b.Property<DateTime>("UpdatedAt").HasColumnType("timestamp with time zone");
                b.Property<string>("Username").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique();
                b.HasIndex("Username").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("backenddemo.ApiService.Models.Flight", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                b.Property<string>("AirlineName").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
                b.Property<string>("FlightNumber").IsRequired().HasMaxLength(20).HasColumnType("character varying(20)");
                b.Property<string>("AircraftType").HasColumnType("text");
                b.Property<string>("FromCity").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
                b.Property<string>("ToCity").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
                b.Property<DateTime>("DepartureTime").HasColumnType("timestamp with time zone");
                b.Property<DateTime>("ArrivalTime").HasColumnType("timestamp with time zone");
                b.Property<int>("DurationMinutes").HasColumnType("integer");
                b.Property<decimal>("Price").HasColumnType("numeric(10,2)");
                b.Property<string>("Currency").IsRequired().HasMaxLength(3).HasColumnType("character varying(3)");
                b.Property<int>("AvailableSeats").HasColumnType("integer");
                b.Property<int>("TotalSeats").HasColumnType("integer");
                b.Property<string>("Status").IsRequired().HasMaxLength(20).HasColumnType("character varying(20)");
                b.Property<string>("Terminal").HasColumnType("text");
                b.Property<string>("Gate").HasColumnType("text");
                b.Property<bool>("IsRefundable").HasColumnType("boolean");
                b.Property<string>("CabinClass").IsRequired().HasMaxLength(20).HasColumnType("character varying(20)");
                b.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
                b.Property<DateTime>("UpdatedAt").HasColumnType("timestamp with time zone");
                b.Property<bool>("IsDeleted").HasColumnType("boolean");
                b.HasKey("Id");
                b.HasIndex("FlightNumber").IsUnique();
                b.ToTable("Flights");
            });
#pragma warning restore 612, 618
        }
    }
}
