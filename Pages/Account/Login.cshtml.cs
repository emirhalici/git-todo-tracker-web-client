using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace git_todo_tracker_web_client.Pages.Account
{
    public class Login : PageModel
    {
        private readonly ILogger<Login> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        private string LoginEndpoint => $"{_configuration["ApiUrl"]}/api/auth/login";

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? ResponseMessage { get; set; }

        public Login(ILogger<Login> logger, IConfiguration configuration, ITokenService tokenService)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var httpClient = new HttpClient();

            var loginData = new
            {
                MailAddress = Email,
                Password = Password
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            using var response = await httpClient.PostAsync(LoginEndpoint, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                // Successful login
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Response 200");
                _logger.LogDebug(result);
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(
                    result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // Set tokens using the token service
                _tokenService.SetTokens(authResponse.AccessToken, authResponse.RefreshToken);

                ResponseMessage = "Login successful. Redirecting...";
                ErrorMessage = null;
                return Page();
            }
            else
            {
                var responseMessage = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObject = JsonSerializer.Deserialize<JsonElement>(responseMessage);
                    errorObject.TryGetProperty("message", out var messageElement);
                    ErrorMessage = messageElement.GetString();
                }
                catch (Exception)
                {
                    ErrorMessage = $"Invalid login attempt with {response.StatusCode}. Full error message:\n{responseMessage}";
                }

                ResponseMessage = null;
                return Page();
            }
        }
    }
}
