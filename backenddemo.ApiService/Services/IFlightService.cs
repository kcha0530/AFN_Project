using backenddemo.ApiService.DTOs;

namespace backenddemo.ApiService.Services;

public interface IFlightService
{
    Task<PagedApiResponse<FlightDto>> GetAllFlightsAsync(FlightQueryParams query);
    Task<ApiResponse<FlightDto>> GetFlightByIdAsync(int id);
    Task<ApiResponse<object>> CreateFlightAsync(CreateFlightDto dto);
    Task<ApiResponse<FlightDto>> UpdateFlightAsync(int id, UpdateFlightDto dto);
    Task<ApiResponse<object>> DeleteFlightAsync(int id);
}
