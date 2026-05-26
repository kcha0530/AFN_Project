namespace backenddemo.ApiService.DTOs;

public record BookingDto(
    int Id,
    int FlightId,
    string FlightNumber,
    string AirlineName,
    string FromCity,
    string ToCity,
    DateTime DepartureTime,
    string PassengerName,
    string PassengerEmail,
    string? PassengerPhone,
    int? UserId,
    int Passengers,
    string CabinClass,
    decimal TotalPrice,
    string BookingReference,
    string Status,
    DateTime CreatedAt
);

public record CreateBookingDto(
    int FlightId,
    string PassengerName,
    string PassengerEmail,
    string? PassengerPhone,
    int? UserId,
    int Passengers,
    string CabinClass
);
