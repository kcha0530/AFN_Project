using System;
using backenddemo.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("backenddemo.ApiService.Models.User", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<string>("Email").IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
                b.Property<string>("FullName").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<bool>("IsActive").HasColumnType("bit");
                b.Property<string>("PasswordHash").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2");
                b.Property<string>("Username").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique();
                b.HasIndex("Username").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("backenddemo.ApiService.Models.Flight", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<string>("AirlineName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("FlightNumber").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<string>("AircraftType").HasColumnType("nvarchar(max)");
                b.Property<string>("FromCity").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("ToCity").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<DateTime>("DepartureTime").HasColumnType("datetime2");
                b.Property<DateTime>("ArrivalTime").HasColumnType("datetime2");
                b.Property<int>("DurationMinutes").HasColumnType("int");
                b.Property<decimal>("Price").HasColumnType("decimal(10,2)");
                b.Property<string>("Currency").IsRequired().HasMaxLength(3).HasColumnType("nvarchar(3)");
                b.Property<int>("AvailableSeats").HasColumnType("int");
                b.Property<int>("TotalSeats").HasColumnType("int");
                b.Property<string>("Status").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<string>("Terminal").HasColumnType("nvarchar(max)");
                b.Property<string>("Gate").HasColumnType("nvarchar(max)");
                b.Property<bool>("IsRefundable").HasColumnType("bit");
                b.Property<string>("CabinClass").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2");
                b.Property<bool>("IsDeleted").HasColumnType("bit");
                b.HasKey("Id");
                b.HasIndex("FlightNumber").IsUnique();
                b.ToTable("Flights");
            });

            modelBuilder.Entity("backenddemo.ApiService.Models.Booking", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<int>("FlightId").HasColumnType("int");
                b.Property<string>("PassengerName").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("PassengerEmail").IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
                b.Property<string>("PassengerPhone").HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<int?>("UserId").HasColumnType("int");
                b.Property<int>("Passengers").HasColumnType("int");
                b.Property<string>("CabinClass").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<decimal>("TotalPrice").HasColumnType("decimal(10,2)");
                b.Property<string>("BookingReference").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<string>("Status").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.HasKey("Id");
                b.HasIndex("FlightId");
                b.HasIndex("BookingReference").IsUnique();
                b.ToTable("Bookings");
            });

            modelBuilder.Entity("backenddemo.ApiService.Models.Booking", b =>
            {
                b.HasOne("backenddemo.ApiService.Models.Flight", "Flight")
                    .WithMany("Bookings")
                    .HasForeignKey("FlightId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
#pragma warning restore 612, 618
        }
    }
}
