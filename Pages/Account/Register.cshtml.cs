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
    public class Register : PageModel
    {
        private readonly ILogger<Register> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        private string RegisterEndpoint => $"{_configuration["ApiUrl"]}/api/auth/register";

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? ResponseMessage { get; set; }

        public Register(ILogger<Register> logger, IConfiguration configuration, ITokenService tokenService)
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
            ErrorMessage = null;
            ResponseMessage = null;
            using var httpClient = new HttpClient();

            var registerData = new
            {
                MailAddress = Email,
                Password = Password
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");

            using var response = await httpClient.PostAsync(RegisterEndpoint, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                // Successful registration
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Response 200");
                _logger.LogDebug(result);

                var authResponse = JsonSerializer.Deserialize<AuthResponse>(
                    result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // Set tokens using the token service
                _tokenService.SetTokens(authResponse.AccessToken, authResponse.RefreshToken);

                ErrorMessage = null;
                return RedirectToPage("/Index");
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
                    ErrorMessage = $"Invalid registration attempt with {response.StatusCode}. Full error message:\n{responseMessage}";
                }

                ResponseMessage = null;
                return Page();
            }
        }
    }
}
