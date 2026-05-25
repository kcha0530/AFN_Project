namespace backenddemo.ApiService.Models;

public class Flight
{
    public int Id { get; set; }
    public string AirlineName { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string? AircraftType { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AUD";
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Terminal { get; set; }
    public string? Gate { get; set; }
    public bool IsRefundable { get; set; } = true;
    public string CabinClass { get; set; } = "Economy";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public void RecalculateDuration() =>
        DurationMinutes = (int)(ArrivalTime - DepartureTime).TotalMinutes;
}
