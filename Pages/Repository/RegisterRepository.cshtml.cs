// RegisterRepository.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace git_todo_tracker_web_client.Pages.Repository
{
    public class RegisterRepositoryModel : PageModel
    {
        private readonly ILogger<RegisterRepositoryModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public RegisterRepositoryModel(ILogger<RegisterRepositoryModel> logger, ITokenService tokenService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public string UserName { get; set; }

        [BindProperty]
        public string ProjectName { get; set; }

        [BindProperty]
        public string GitRepositoryUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task OnPostAsync()
        {
            // Ensure the user is authenticated and has an access token
            if (string.IsNullOrEmpty(_tokenService.AccessToken))
            {
                // Redirect to login if not authenticated
                Response.Redirect("/Account/Login");
                return;
            }

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);

            var endpoint = $"{_configuration["ApiUrl"]}/api/repository/register";

            var requestBody = new
            {
                UserName,
                ProjectName,
                GitRepositoryUrl
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Repository registration successful
                Response.Redirect("/Index");
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
                    ErrorMessage = $"Error registering repository with {response.StatusCode}. Full error message:\n{responseMessage}";
                }
            }
        }
    }
}
