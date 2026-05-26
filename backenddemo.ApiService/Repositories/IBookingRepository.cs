using backenddemo.ApiService.Models;

namespace backenddemo.ApiService.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<Booking?> GetByReferenceAsync(string reference);
    Task<IEnumerable<Booking>> GetByEmailAsync(string email);
    Task<Booking> CreateAsync(Booking booking);
    Task<bool> ExistsAsync(string reference);
}
