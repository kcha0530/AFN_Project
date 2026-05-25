using backenddemo.ApiService.DTOs;
using backenddemo.ApiService.Models;
using backenddemo.ApiService.Repositories;
using Microsoft.Extensions.Logging;

namespace backenddemo.ApiService.Services;

public class FlightService(IFlightRepository repo, ILogger<FlightService> logger) : IFlightService
{
    public async Task<PagedApiResponse<FlightDto>> GetAllFlightsAsync(FlightQueryParams query)
    {
        var (flights, total) = await repo.GetAllAsync(query);
        var dtos = flights.Select(MapToDto);
        logger.LogInformation("Fetched {Count}/{Total} flights (page {Page})", dtos.Count(), total, query.Page);
        return PagedApiResponse<FlightDto>.Ok(dtos, query.Page, query.PageSize, total, "Flights fetched successfully");
    }

    public async Task<ApiResponse<FlightDto>> GetFlightByIdAsync(int id)
    {
        var flight = await repo.GetByIdAsync(id);
        if (flight == null)
            return ApiResponse<FlightDto>.Fail("Flight not found");
        return ApiResponse<FlightDto>.Ok(MapToDto(flight), "Flight found");
    }

    public async Task<ApiResponse<object>> CreateFlightAsync(CreateFlightDto dto)
    {
        var errors = ValidateCreate(dto);
        if (errors.Count > 0)
        {
            logger.LogWarning("Flight creation validation failed: {Errors}", string.Join("; ", errors));
            return ApiResponse<object>.Fail("Validation failed", errors);
        }

        var existing = await repo.GetByFlightNumberAsync(dto.FlightNumber);
        if (existing != null)
            return ApiResponse<object>.Fail("Validation failed", ["Flight number already exists"]);

        var flight = new Flight
        {
            AirlineName     = dto.AirlineName,
            FlightNumber    = dto.FlightNumber.ToUpperInvariant(),
            AircraftType    = dto.AircraftType,
            FromCity        = dto.FromCity,
            ToCity          = dto.ToCity,
            DepartureTime   = dto.DepartureTime,
            ArrivalTime     = dto.ArrivalTime,
            Price           = dto.Price,
            Currency        = dto.Currency,
            TotalSeats      = dto.TotalSeats,
            AvailableSeats  = dto.AvailableSeats,
            Status          = dto.Status,
            Terminal        = dto.Terminal,
            Gate            = dto.Gate,
            IsRefundable    = dto.IsRefundable,
            CabinClass      = dto.CabinClass,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow
        };
        flight.RecalculateDuration();

        var created = await repo.CreateAsync(flight);
        logger.LogInformation("Created flight {FlightNumber} (Id={Id})", created.FlightNumber, created.Id);
        return ApiResponse<object>.Ok(new { id = created.Id }, "Flight created successfully");
    }

    public async Task<ApiResponse<FlightDto>> UpdateFlightAsync(int id, UpdateFlightDto dto)
    {
        var flight = await repo.GetByIdAsync(id);
        if (flight == null)
            return ApiResponse<FlightDto>.Fail("Flight not found");

        // Apply only provided fields
        if (dto.AirlineName    != null) flight.AirlineName   = dto.AirlineName;
        if (dto.AircraftType   != null) flight.AircraftType  = dto.AircraftType;
        if (dto.FromCity       != null) flight.FromCity      = dto.FromCity;
        if (dto.ToCity         != null) flight.ToCity        = dto.ToCity;
        if (dto.Price          != null) flight.Price         = dto.Price.Value;
        if (dto.TotalSeats     != null) flight.TotalSeats    = dto.TotalSeats.Value;
        if (dto.AvailableSeats != null) flight.AvailableSeats= dto.AvailableSeats.Value;
        if (dto.Currency       != null) flight.Currency      = dto.Currency;
        if (dto.Status         != null) flight.Status        = dto.Status;
        if (dto.Terminal       != null) flight.Terminal      = dto.Terminal;
        if (dto.Gate           != null) flight.Gate          = dto.Gate;
        if (dto.IsRefundable   != null) flight.IsRefundable  = dto.IsRefundable.Value;
        if (dto.CabinClass     != null) flight.CabinClass    = dto.CabinClass;
        if (dto.DepartureTime  != null) flight.DepartureTime = dto.DepartureTime.Value;
        if (dto.ArrivalTime    != null) flight.ArrivalTime   = dto.ArrivalTime.Value;

        var updateErrors = ValidateUpdate(flight);
        if (updateErrors.Count > 0)
            return ApiResponse<FlightDto>.Fail("Validation failed", updateErrors);

        flight.RecalculateDuration();
        var updated = await repo.UpdateAsync(flight);
        logger.LogInformation("Updated flight {Id}", id);
        return ApiResponse<FlightDto>.Ok(MapToDto(updated), "Flight updated successfully");
    }

    public async Task<ApiResponse<object>> DeleteFlightAsync(int id)
    {
        var deleted = await repo.SoftDeleteAsync(id);
        if (!deleted)
            return ApiResponse<object>.Fail("Flight not found");
        logger.LogInformation("Soft-deleted flight {Id}", id);
        return ApiResponse<object>.Ok(new { }, "Flight deleted successfully");
    }

    // ── Validation ────────────────────────────────────────────────────────────

    private static List<string> ValidateCreate(CreateFlightDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.AirlineName))   errors.Add("AirlineName is required");
        if (string.IsNullOrWhiteSpace(dto.FlightNumber))  errors.Add("FlightNumber is required");
        if (string.IsNullOrWhiteSpace(dto.FromCity))      errors.Add("FromCity is required");
        if (string.IsNullOrWhiteSpace(dto.ToCity))        errors.Add("ToCity is required");
        if (dto.Price < 0)                                errors.Add("Price must be greater than or equal to 0");
        if (dto.TotalSeats <= 0)                          errors.Add("TotalSeats must be greater than 0");
        if (dto.AvailableSeats < 0)                       errors.Add("AvailableSeats cannot be negative");
        if (dto.AvailableSeats > dto.TotalSeats)          errors.Add("AvailableSeats cannot exceed TotalSeats");
        if (dto.ArrivalTime <= dto.DepartureTime)         errors.Add("ArrivalTime must be after DepartureTime");
        return errors;
    }

    private static List<string> ValidateUpdate(Flight f)
    {
        var errors = new List<string>();
        if (f.Price < 0)                          errors.Add("Price must be greater than or equal to 0");
        if (f.AvailableSeats > f.TotalSeats)      errors.Add("AvailableSeats cannot exceed TotalSeats");
        if (f.ArrivalTime <= f.DepartureTime)     errors.Add("ArrivalTime must be after DepartureTime");
        return errors;
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static FlightDto MapToDto(Flight f) => new(
        f.Id, f.AirlineName, f.FlightNumber, f.AircraftType,
        f.FromCity, f.ToCity, f.DepartureTime, f.ArrivalTime,
        f.DurationMinutes, f.Price, f.Currency, f.AvailableSeats,
        f.TotalSeats, f.Status, f.Terminal, f.Gate,
        f.IsRefundable, f.CabinClass, f.CreatedAt, f.UpdatedAt
    );
}
