using backenddemo.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace backenddemo.ApiService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Flight> Flights { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AirlineName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FromCity).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ToCity).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.CabinClass).HasMaxLength(20);
            entity.HasIndex(e => e.FlightNumber).IsUnique();
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PassengerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PassengerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PassengerPhone).HasMaxLength(20);
            entity.Property(e => e.CabinClass).HasMaxLength(20);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.BookingReference).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasIndex(e => e.BookingReference).IsUnique();
            entity.HasOne(e => e.Flight)
                  .WithMany(f => f.Bookings)
                  .HasForeignKey(e => e.FlightId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
