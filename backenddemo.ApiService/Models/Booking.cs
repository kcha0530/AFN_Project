namespace backenddemo.ApiService.Models;

public class Booking
{
    public int Id { get; set; }
    public int FlightId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string? PassengerPhone { get; set; }
    public int? UserId { get; set; }
    public int Passengers { get; set; } = 1;
    public string CabinClass { get; set; } = "Economy";
    public decimal TotalPrice { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Flight? Flight { get; set; }
}
