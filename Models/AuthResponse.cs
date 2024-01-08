public class AuthResponse
{
    public required string? AccessToken { get; set; }

    public required string? RefreshToken { get; set; }

    public required string Message { get; set; }
    public required int StatusCode { get; set; }
}