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
        if (!IsLoginPage(context))
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

    private static bool IsLoginPage(PageHandlerExecutingContext context)
    {
        // Adjust the condition based on your Razor Pages routing pattern
        return context.ActionDescriptor.RouteValues.ContainsKey("page") &&
               context.ActionDescriptor.RouteValues["page"]?.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase) == true;
    }
}
