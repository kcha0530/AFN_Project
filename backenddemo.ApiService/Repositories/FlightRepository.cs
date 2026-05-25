using backenddemo.ApiService.Data;
using backenddemo.ApiService.DTOs;
using backenddemo.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace backenddemo.ApiService.Repositories;

public class FlightRepository(ApplicationDbContext db) : IFlightRepository
{
    public async Task<(IEnumerable<Flight> Flights, int TotalCount)> GetAllAsync(FlightQueryParams q)
    {
        var query = db.Flights.Where(f => !f.IsDeleted).AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(q.FromCity))
            query = query.Where(f => f.FromCity.ToLower().Contains(q.FromCity.ToLower()));
        if (!string.IsNullOrWhiteSpace(q.ToCity))
            query = query.Where(f => f.ToCity.ToLower().Contains(q.ToCity.ToLower()));
        if (!string.IsNullOrWhiteSpace(q.Airline))
            query = query.Where(f => f.AirlineName.ToLower().Contains(q.Airline.ToLower()));
        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(f => f.Status == q.Status);
        if (!string.IsNullOrWhiteSpace(q.CabinClass))
            query = query.Where(f => f.CabinClass == q.CabinClass);
        if (q.MinPrice.HasValue)
            query = query.Where(f => f.Price >= q.MinPrice.Value);
        if (q.MaxPrice.HasValue)
            query = query.Where(f => f.Price <= q.MaxPrice.Value);

        // Search across multiple fields
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            query = query.Where(f =>
                f.FlightNumber.ToLower().Contains(s) ||
                f.AirlineName.ToLower().Contains(s) ||
                f.FromCity.ToLower().Contains(s) ||
                f.ToCity.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        // Sorting
        query = q.SortBy?.ToLower() switch
        {
            "price"         => q.SortDir == "desc" ? query.OrderByDescending(f => f.Price)         : query.OrderBy(f => f.Price),
            "airline"       => q.SortDir == "desc" ? query.OrderByDescending(f => f.AirlineName)   : query.OrderBy(f => f.AirlineName),
            "fromcity"      => q.SortDir == "desc" ? query.OrderByDescending(f => f.FromCity)      : query.OrderBy(f => f.FromCity),
            "tocity"        => q.SortDir == "desc" ? query.OrderByDescending(f => f.ToCity)        : query.OrderBy(f => f.ToCity),
            "arrivaltime"   => q.SortDir == "desc" ? query.OrderByDescending(f => f.ArrivalTime)   : query.OrderBy(f => f.ArrivalTime),
            "availableseats"=> q.SortDir == "desc" ? query.OrderByDescending(f => f.AvailableSeats): query.OrderBy(f => f.AvailableSeats),
            _               => q.SortDir == "desc" ? query.OrderByDescending(f => f.DepartureTime) : query.OrderBy(f => f.DepartureTime),
        };

        // Pagination
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);
        var flights = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (flights, totalCount);
    }

    public async Task<Flight?> GetByIdAsync(int id) =>
        await db.Flights.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

    public async Task<Flight?> GetByFlightNumberAsync(string flightNumber) =>
        await db.Flights.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber && !f.IsDeleted);

    public async Task<Flight> CreateAsync(Flight flight)
    {
        db.Flights.Add(flight);
        await db.SaveChangesAsync();
        return flight;
    }

    public async Task<Flight> UpdateAsync(Flight flight)
    {
        flight.UpdatedAt = DateTime.UtcNow;
        db.Flights.Update(flight);
        await db.SaveChangesAsync();
        return flight;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var flight = await db.Flights.FindAsync(id);
        if (flight == null) return false;
        flight.IsDeleted = true;
        flight.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await db.Flights.AnyAsync(f => f.Id == id && !f.IsDeleted);
}
