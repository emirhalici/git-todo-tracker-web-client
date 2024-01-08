using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace git_todo_tracker_web_client.Pages.Account
{
    public class Logout : PageModel
    {
        private readonly ILogger<Logout> _logger;
        private readonly ITokenService _tokenService;


        public Logout(ILogger<Logout> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Clear tokens when logging out
            _tokenService.ClearTokens();

            // Redirect to the login page after logout
            return RedirectToPage("/Account/Login");
        }
    }
}