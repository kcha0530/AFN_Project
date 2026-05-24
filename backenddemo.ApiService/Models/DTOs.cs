namespace backenddemo.ApiService.Models;

public record UserDto(int Id, string Username, string Email, string FullName, DateTime CreatedAt);

public record UserProfileUpdateRequest(string Email, string FullName);

public record UserRegisterRequest(string Username, string Email, string Password, string FullName);

public record DashboardStatsDto(
    int TotalUsers,
    int TotalProducts,
    int ActiveUsers,
    DateTime LastUpdated
);

public record ApiHealthDto(
    string Status,
    string Version,
    DateTime Timestamp,
    string Database
);
