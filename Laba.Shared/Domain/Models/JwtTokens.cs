namespace Laba.Shared.Domain.Models;

public readonly record struct JwtTokens(string AccessToken, string RefreshToken);