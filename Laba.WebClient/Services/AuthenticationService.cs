using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Laba.Shared.Domain.Dtos;
using Laba.Shared.Domain.Models;
using Laba.Shared.Exceptions;
using Laba.Shared.Requests;
using Laba.WebClient.Abstract.ServiceInterfaces;

namespace Laba.WebClient.Services;

public class AuthenticationService(HttpClient httpClient, LocalStorageService localStorage) : IAuthService
{
    public async Task<UserDtoToView> GetInfoAboutUser()
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/get-info");
        string? accessToken = await localStorage.GetAccessToken();

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new UnauthorizedAccessException("The user doesn't have access token in local storage.");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", parameter: accessToken);
        var response = await httpClient.SendAsync(httpRequest);

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserDtoToView>();
        return user!;
    }

    public async Task Register(RegisterRequest request)
    {
        var contentForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = request.Username,
            ["Email"] = request.Email,
            ["Password"] = request.Password
        });
        var response = await httpClient.PostAsync("api/auth/register", contentForm);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var conflictContent = await response.Content.ReadFromJsonAsync<ErrorOfProperty>();

            if (conflictContent != null)
                throw new InvalidPropertyException(conflictContent.ErrorMessage, HttpStatusCode.Conflict,
                    response.Headers, conflictContent);
        }

        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<JwtTokens>();

        await localStorage.Save(LocalStorageService.AccessTokenPropertyName, tokens.AccessToken);
        await localStorage.Save(LocalStorageService.RefreshTokenPropertyName, tokens.RefreshToken);
    }

    public async Task Login(LoginRequest request)
    {
        var contentForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = request.Email,
            ["Password"] = request.Password
        });
        var response = await httpClient.PostAsync("api/auth/login", contentForm);

        switch (response.StatusCode)
        {
            case HttpStatusCode.TooManyRequests:
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new TooManyRequestsException(errorText, HttpStatusCode.TooManyRequests, response.Headers,
                    errorText);
            }
            case HttpStatusCode.BadRequest:
            {
                var invalidPropertyContent = await response.Content.ReadFromJsonAsync<ErrorOfProperty>();

                if (invalidPropertyContent != null)
                    throw new InvalidPropertyException(invalidPropertyContent.ErrorMessage, HttpStatusCode.Conflict,
                        response.Headers, invalidPropertyContent);
                break;
            }
            case HttpStatusCode.NotFound:
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new NotFoundException(errorText, HttpStatusCode.TooManyRequests, response.Headers,
                    errorText);
            }
        }

        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<JwtTokens>();

        await localStorage.Save(LocalStorageService.AccessTokenPropertyName, tokens.AccessToken);
        await localStorage.Save(LocalStorageService.RefreshTokenPropertyName, tokens.RefreshToken);
    }

    public async Task Logout()
    {
        await localStorage.Remove(LocalStorageService.AccessTokenPropertyName);
        await localStorage.Remove(LocalStorageService.RefreshTokenPropertyName);
    }
}