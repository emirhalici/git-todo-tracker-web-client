public interface ITokenService
{
    string? AccessToken { get; }
    string? RefreshToken { get; }

    void SetTokens(string accessToken, string refreshToken);
    void ClearTokens();
}

public class TokenService : ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? AccessToken
    {
        get => _httpContextAccessor.HttpContext.Request.Cookies["accessToken"];
        private set => _httpContextAccessor.HttpContext.Response.Cookies.Append("accessToken", value ?? string.Empty);
    }

    public string? RefreshToken
    {
        get => _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
        private set => _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", value ?? string.Empty);
    }

    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public void ClearTokens()
    {
        AccessToken = null;
        RefreshToken = null;
        _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken");
        _httpContextAccessor.HttpContext.Response.Cookies.Delete("accessToken");
    }
}
