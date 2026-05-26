using backenddemo.ApiService.DTOs;
using backenddemo.ApiService.Models;
using backenddemo.ApiService.Repositories;

namespace backenddemo.ApiService.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IFlightRepository _flightRepo;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository bookingRepo, IFlightRepository flightRepo, ILogger<BookingService> logger)
    {
        _bookingRepo = bookingRepo;
        _flightRepo = flightRepo;
        _logger = logger;
    }

    public async Task<(BookingDto? booking, string? error)> CreateBookingAsync(CreateBookingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PassengerName))
            return (null, "Passenger name is required.");
        if (string.IsNullOrWhiteSpace(dto.PassengerEmail) || !dto.PassengerEmail.Contains('@'))
            return (null, "A valid passenger email is required.");
        if (dto.Passengers < 1 || dto.Passengers > 9)
            return (null, "Passengers must be between 1 and 9.");

        var flight = await _flightRepo.GetByIdAsync(dto.FlightId);
        if (flight == null || flight.IsDeleted)
            return (null, "Flight not found.");
        if (flight.Status == "Cancelled")
            return (null, "Cannot book a cancelled flight.");
        if (flight.AvailableSeats < dto.Passengers)
            return (null, $"Only {flight.AvailableSeats} seat(s) available.");

        var totalPrice = flight.Price * dto.Passengers;
        var reference = GenerateReference();

        var booking = new Booking
        {
            FlightId = dto.FlightId,
            PassengerName = dto.PassengerName.Trim(),
            PassengerEmail = dto.PassengerEmail.Trim().ToLower(),
            PassengerPhone = dto.PassengerPhone?.Trim(),
            UserId = dto.UserId,
            Passengers = dto.Passengers,
            CabinClass = dto.CabinClass,
            TotalPrice = totalPrice,
            BookingReference = reference,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        flight.AvailableSeats -= dto.Passengers;
        flight.UpdatedAt = DateTime.UtcNow;

        var created = await _bookingRepo.CreateAsync(booking);
        _logger.LogInformation("Booking {Ref} created for flight {FlightId}", reference, dto.FlightId);

        return (MapToDto(created), null);
    }

    public async Task<BookingDto?> GetByReferenceAsync(string reference)
    {
        var b = await _bookingRepo.GetByReferenceAsync(reference);
        return b == null ? null : MapToDto(b);
    }

    public async Task<IEnumerable<BookingDto>> GetByEmailAsync(string email)
    {
        var bookings = await _bookingRepo.GetByEmailAsync(email);
        return bookings.Select(MapToDto);
    }

    private static string GenerateReference()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = new Random();
        return new string(Enumerable.Range(0, 8).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
    }

    private static BookingDto MapToDto(Booking b) => new(
        b.Id, b.FlightId,
        b.Flight?.FlightNumber ?? "",
        b.Flight?.AirlineName ?? "",
        b.Flight?.FromCity ?? "",
        b.Flight?.ToCity ?? "",
        b.Flight?.DepartureTime ?? DateTime.MinValue,
        b.PassengerName, b.PassengerEmail, b.PassengerPhone,
        b.UserId, b.Passengers, b.CabinClass,
        b.TotalPrice, b.BookingReference, b.Status, b.CreatedAt
    );
}
