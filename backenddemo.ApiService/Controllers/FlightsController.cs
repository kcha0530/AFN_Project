using backenddemo.ApiService.DTOs;
using backenddemo.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backenddemo.ApiService.Controllers;

[ApiController]
[Route("api/flights")]
[Produces("application/json")]
public class FlightsController(IFlightService service) : ControllerBase
{
    /// <summary>Get all flights with pagination, filtering, sorting, and search.</summary>
    /// <remarks>
    /// Example: GET /api/flights?page=1&amp;pageSize=10&amp;fromCity=Melbourne&amp;toCity=Bangkok&amp;sortBy=price
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<FlightDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FlightQueryParams query)
    {
        var result = await service.GetAllFlightsAsync(query);
        return Ok(result);
    }

    /// <summary>Get a single flight by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<FlightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlightDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetFlightByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new flight. Requires authentication.</summary>
    /// <remarks>
    /// Business rules: FlightNumber must be unique, Price >= 0, ArrivalTime > DepartureTime,
    /// AvailableSeats &lt;= TotalSeats. DurationMinutes is auto-calculated.
    /// </remarks>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFlightDto dto)
    {
        var result = await service.CreateFlightAsync(dto);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = (result.Data as dynamic)!.id }, result);
    }

    /// <summary>Update an existing flight. Requires authentication. Only provided fields are updated.</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FlightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlightDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FlightDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFlightDto dto)
    {
        var result = await service.UpdateFlightAsync(id, dto);
        if (!result.Success)
            return result.Errors != null ? BadRequest(result) : NotFound(result);
        return Ok(result);
    }

    /// <summary>Soft-delete a flight (marks IsDeleted=true). Requires authentication.</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteFlightAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
