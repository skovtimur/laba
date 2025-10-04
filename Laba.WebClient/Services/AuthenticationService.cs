using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Laba.Shared.Domain.Dtos;
using Laba.Shared.Domain.Models;
using Laba.Shared.Exceptions;
using Laba.Shared.Requests;
using Laba.Shared.Services;
using Laba.WebClient.Abstract.ServiceInterfaces;
using Laba.WebClient.Exceptions;

namespace Laba.WebClient.Services;

public class AuthenticationService(
    HttpClient httpClient,
    ILogger<AuthenticationService> logger,
    LocalStorageService localStorage,
    IHash hash) : IAuthService
{
    public async Task<UserDtoToView> GetInfoAboutUser()
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/get-info");
        string? accessToken = await localStorage.GetAccessToken();

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new UnauthorizedAccessException("The user doesn't have access token in local storage.");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", parameter: accessToken);
        var response = await httpClient.SendAsync(httpRequest);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new ExpiredAccessToken();

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
                throw new ProblemOfPropertyException(conflictContent.ErrorMessage, HttpStatusCode.Conflict,
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
                    throw new ProblemOfPropertyException(invalidPropertyContent.ErrorMessage, HttpStatusCode.BadRequest,
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
        await SaveTokens(tokens);
    }

    public async Task Logout()
    {
        await localStorage.Remove(LocalStorageService.AccessTokenPropertyName);
        await localStorage.Remove(LocalStorageService.RefreshTokenPropertyName);
    }

    public async Task UpdateTokens(string oldRefreshToken)
    {
        var response = await httpClient.PutAsync($"api/auth/tokens-update/{oldRefreshToken}", null);
        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<JwtTokens>();
        await SaveTokens(tokens);
    }

    public async Task<ChallengeModel> StartHandshake(string email)
    {
        var response = await httpClient.PostAsync($"api/auth/handshake/start/{email}", null);
        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
            {
                var invalidPropertyContent = await response.Content.ReadFromJsonAsync<ErrorOfProperty>();

                if (invalidPropertyContent != null)
                    throw new ProblemOfPropertyException(invalidPropertyContent.ErrorMessage, HttpStatusCode.BadRequest,
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

        var data = await response.Content.ReadFromJsonAsync<ChallengeModel>();
        return data;
    }

    public async Task EndHandshake(ChallengeModel challenge, string email, string password)
    {
        var sw = Stopwatch.StartNew();

        // Раз соль и пороль один и тот же значит все будет ОК
        // Если пороль не верный значит и хэш будет не тем
        var hashedPassword = hash.HashPassword(password, Convert.FromHexString(challenge.SaltHex));
        var response = hashedPassword.Split("-")[0];

        logger.LogInformation("1) computeHandshakeResponse.ComputeHandshakeResponse(): {ms} ms",
            sw.ElapsedMilliseconds);

        var finishResponse = await httpClient.PostAsync("api/auth/handshake/end",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [nameof(EndHandshakeRequest.Email)] = email,
                [nameof(EndHandshakeRequest.HashedResponse)] = response,
            }));

        logger.LogInformation("Content: {x}", finishResponse.Content.ToString());
        logger.LogInformation("Content: {x}", finishResponse.Content);
        logger.LogInformation("2) httpClient.PostAsync(): {ms} ms", sw.ElapsedMilliseconds);
        switch (finishResponse.StatusCode)
        {
            case HttpStatusCode.TooManyRequests:
            {
                var errorText = await finishResponse.Content.ReadAsStringAsync();
                throw new TooManyRequestsException(errorText, HttpStatusCode.TooManyRequests, finishResponse.Headers,
                    errorText);
            }
            case HttpStatusCode.BadRequest:
            {
                ErrorOfProperty? invalidPropertyContent =
                    await finishResponse.Content.ReadFromJsonAsync<ErrorOfProperty>();

                if (invalidPropertyContent != null &&
                    string.IsNullOrEmpty(invalidPropertyContent.PropertyName) == false)
                    throw new ProblemOfPropertyException(invalidPropertyContent.ErrorMessage, HttpStatusCode.BadRequest,
                        finishResponse.Headers, invalidPropertyContent);

                var errorText = await finishResponse.Content.ReadAsStringAsync();
                throw new BadRequestException(errorText, HttpStatusCode.BadRequest, finishResponse.Headers,
                    errorText);
            }
            case HttpStatusCode.NotFound:
            {
                var errorText = await finishResponse.Content.ReadAsStringAsync();
                throw new NotFoundException(errorText, HttpStatusCode.TooManyRequests, finishResponse.Headers,
                    errorText);
            }
        }

        finishResponse.EnsureSuccessStatusCode();

        var tokens = await finishResponse.Content.ReadFromJsonAsync<JwtTokens>();
        logger.LogInformation("3) finishResponse.Content.ReadFromJsonAsync<JwtTokens>(): {ms} ms",
            sw.ElapsedMilliseconds);

        await SaveTokens(tokens);
        logger.LogInformation("END: {ms} ms", sw.ElapsedMilliseconds);
    }

    private async Task SaveTokens(JwtTokens tokens)
    {
        await localStorage.Save(LocalStorageService.AccessTokenPropertyName, tokens.AccessToken);
        await localStorage.Save(LocalStorageService.RefreshTokenPropertyName, tokens.RefreshToken);
    }
}