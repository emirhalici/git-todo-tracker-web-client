using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml;


namespace git_todo_tracker_web_client.Pages.Account
{
    public class Login : PageModel
    {
        private readonly ILogger<Login> _logger;
        private readonly IConfiguration _configuration;
        private string LoginEndpoint => $"{_configuration["ApiUrl"]}/api/auth/login";

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? ResponseMessage { get; set; }

        public Login(ILogger<Login> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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