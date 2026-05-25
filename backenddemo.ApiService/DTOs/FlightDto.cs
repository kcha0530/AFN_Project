namespace backenddemo.ApiService.DTOs;

public record FlightDto(
    int Id,
    string AirlineName,
    string FlightNumber,
    string? AircraftType,
    string FromCity,
    string ToCity,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    int DurationMinutes,
    decimal Price,
    string Currency,
    int AvailableSeats,
    int TotalSeats,
    string Status,
    string? Terminal,
    string? Gate,
    bool IsRefundable,
    string CabinClass,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateFlightDto(
    string AirlineName,
    string FlightNumber,
    string? AircraftType,
    string FromCity,
    string ToCity,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal Price,
    int TotalSeats,
    int AvailableSeats,
    string Currency = "AUD",
    string Status = "Scheduled",
    string? Terminal = null,
    string? Gate = null,
    bool IsRefundable = true,
    string CabinClass = "Economy"
);

public record UpdateFlightDto(
    string? AirlineName,
    string? AircraftType,
    string? FromCity,
    string? ToCity,
    DateTime? DepartureTime,
    DateTime? ArrivalTime,
    decimal? Price,
    int? TotalSeats,
    int? AvailableSeats,
    string? Currency,
    string? Status,
    string? Terminal,
    string? Gate,
    bool? IsRefundable,
    string? CabinClass
);

public class FlightQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? FromCity { get; set; }
    public string? ToCity { get; set; }
    public string? Airline { get; set; }
    public string? Status { get; set; }
    public string? CabinClass { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "departureTime";
    public string SortDir { get; set; } = "asc";
}
