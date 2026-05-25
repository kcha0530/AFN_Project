using backenddemo.ApiService.DTOs;
using backenddemo.ApiService.Models;

namespace backenddemo.ApiService.Repositories;

public interface IFlightRepository
{
    Task<(IEnumerable<Flight> Flights, int TotalCount)> GetAllAsync(FlightQueryParams query);
    Task<Flight?> GetByIdAsync(int id);
    Task<Flight?> GetByFlightNumberAsync(string flightNumber);
    Task<Flight> CreateAsync(Flight flight);
    Task<Flight> UpdateAsync(Flight flight);
    Task<bool> SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
