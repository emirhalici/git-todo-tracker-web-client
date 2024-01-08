using git_todo_tracker_web_client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace git_todo_tracker_web_client.Pages.Repository
{
    public class RepositoryTodosModel : PageModel
    {
        private readonly ILogger<RepositoryTodosModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public RepositoryTodosModel(ILogger<RepositoryTodosModel> logger, ITokenService tokenService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<TodoModel> Todos { get; set; }

        [BindProperty(SupportsGet = true)]
        public string RepoId { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Ensure the user is authenticated and has an access token
            if (string.IsNullOrEmpty(_tokenService.AccessToken))
            {
                // Redirect to login if not authenticated
                Response.Redirect("/Account/Login");
                return;
            }

            await LoadTodosAsync();
        }

        private async Task LoadTodosAsync()
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);

            var endpoint = $"{_configuration["ApiUrl"]}/api/todo/list?repoId={RepoId}";

            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug(content);
                Todos = JsonSerializer.Deserialize<List<TodoModel>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
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
                    ErrorMessage = $"Error loading todos with {response.StatusCode}. Full error message:\n{responseMessage}";
                }
                Todos = new List<TodoModel>();
            }
        }
    }
}
