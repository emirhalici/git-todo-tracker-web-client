using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class TokenAuthorizationFilter : IAsyncPageFilter
{
    private readonly ITokenService _tokenService;

    public TokenAuthorizationFilter(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // Check if the current page is not the login page
        if (!IsUnauthenticatedPage(context))
        {
            // Check if tokens exist in cookies
            if (string.IsNullOrEmpty(_tokenService.AccessToken) || string.IsNullOrEmpty(_tokenService.RefreshToken))
            {
                context.Result = new RedirectToPageResult("/Account/Login");
                return;
            }
        }

        await next();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    private static bool IsUnauthenticatedPage(PageHandlerExecutingContext context)
    {
        var allowedPages = new List<String> { "/Account/Login", "/Account/Register" };

        if (!context.ActionDescriptor.RouteValues.ContainsKey("page")) return false;

        foreach (var page in allowedPages)
        {
            if (context.ActionDescriptor.RouteValues["page"]?.Equals(page, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
        }
        return false;
    }
}
