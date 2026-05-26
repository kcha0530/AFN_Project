using backenddemo.ApiService.DTOs;

namespace backenddemo.ApiService.Services;

public interface IBookingService
{
    Task<(BookingDto? booking, string? error)> CreateBookingAsync(CreateBookingDto dto);
    Task<BookingDto?> GetByReferenceAsync(string reference);
    Task<IEnumerable<BookingDto>> GetByEmailAsync(string email);
}
