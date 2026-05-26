using backenddemo.ApiService.Data;
using backenddemo.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace backenddemo.ApiService.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _db;

    public BookingRepository(ApplicationDbContext db) => _db = db;

    public Task<Booking?> GetByIdAsync(int id) =>
        _db.Bookings.Include(b => b.Flight).FirstOrDefaultAsync(b => b.Id == id);

    public Task<Booking?> GetByReferenceAsync(string reference) =>
        _db.Bookings.Include(b => b.Flight).FirstOrDefaultAsync(b => b.BookingReference == reference);

    public async Task<IEnumerable<Booking>> GetByEmailAsync(string email) =>
        await _db.Bookings.Include(b => b.Flight)
            .Where(b => b.PassengerEmail == email)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<Booking> CreateAsync(Booking booking)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        await _db.Entry(booking).Reference(b => b.Flight).LoadAsync();
        return booking;
    }

    public Task<bool> ExistsAsync(string reference) =>
        _db.Bookings.AnyAsync(b => b.BookingReference == reference);
}
