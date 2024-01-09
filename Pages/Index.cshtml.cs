using System.Net.Http.Headers;
using System.Text.Json;
using git_todo_tracker_web_client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;


namespace git_todo_tracker_web_client.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    private string GetRepositoriesEndpoint => $"{_configuration["ApiUrl"]}/api/repository/list";

    public IndexModel(ILogger<IndexModel> logger, ITokenService tokenService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _tokenService = tokenService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public List<RepositoryModel> Repositories { get; set; }

    [BindProperty]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Ensure the user is authenticated and has an access token
        if (string.IsNullOrEmpty(_tokenService.AccessToken))
        {
            // Redirect to login if not authenticated
            Response.Redirect("/Account/Login");
            return;
        }

        await LoadRepositoriesAsync();
    }

    // TODO: This todo message should be visible from the web app!

    private async Task LoadRepositoriesAsync()
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.AccessToken);

            var response = await client.GetAsync(GetRepositoriesEndpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug(content);
                Repositories = JsonSerializer.Deserialize<List<RepositoryModel>>(content,
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
                    ErrorMessage = $"Invalid login attempt with {response.StatusCode}. Full error message:\n{responseMessage}";
                }
                Repositories = new List<RepositoryModel>();
            }
        }
        catch (Exception e)
        {
            ErrorMessage = $"Unexpected error occured:\n{e.Message}";

        }

    }
}

