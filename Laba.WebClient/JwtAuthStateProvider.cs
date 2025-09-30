using System.Globalization;
using System.Security.Claims;
using Laba.Shared.Domain.Dtos;
using Laba.Shared.Domain.Models;
using Laba.WebClient.Abstract.ServiceInterfaces;
using Laba.WebClient.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Laba.WebClient;

public class JwtAuthStateProvider(
    ILogger<JwtAuthStateProvider> logger,
    IAuthService authService)
    : AuthenticationStateProvider
{
    //AuthenticationStateProvider используется приложением чтобы понять состояние аутентификации.
    
    private UserDtoToView? _userDto = null;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        try
        {
            var user = _userDto;

            if (_userDto == null)
                user = await authService.GetInfoAboutUser();

            var identity = new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Username),
                new(ClaimTypes.DateOfBirth, user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
            }, "Bearer");

            var principal = new ClaimsPrincipal(identity);
            state = new AuthenticationState(principal);
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception.Message);
        }

        NotifyAuthenticationStateChanged(Task.FromResult(state));
        return state;
    }
}