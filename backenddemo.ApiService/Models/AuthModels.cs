namespace backenddemo.ApiService.Models;

public record UserLogin(string Username, string Password);

public record JwtSettings(string Key, string Issuer, string Audience, int ExpireMinutes);
