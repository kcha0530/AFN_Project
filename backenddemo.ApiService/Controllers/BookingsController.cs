using backenddemo.ApiService.DTOs;
using backenddemo.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace backenddemo.ApiService.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService service, ILogger<BookingsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Create a booking (guest or authenticated)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var (booking, error) = await _service.CreateBookingAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<object>.Fail(error));

        return CreatedAtAction(nameof(GetByReference), new { reference = booking!.BookingReference },
            ApiResponse<BookingDto>.Ok(booking, "Booking confirmed."));
    }

    /// <summary>Look up a booking by reference code</summary>
    [HttpGet("{reference}")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(string reference)
    {
        var booking = await _service.GetByReferenceAsync(reference.ToUpper());
        if (booking == null)
            return NotFound(ApiResponse<object>.Fail("Booking not found."));

        return Ok(ApiResponse<BookingDto>.Ok(booking));
    }

    /// <summary>Get all bookings for a passenger email</summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var bookings = await _service.GetByEmailAsync(email.ToLower());
        return Ok(ApiResponse<IEnumerable<BookingDto>>.Ok(bookings));
    }
}
