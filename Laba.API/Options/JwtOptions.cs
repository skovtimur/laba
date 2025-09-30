namespace Laba.API.Options;

public class JwtOptions
{
    public required string Issuer { get; set; }
    
    public required string AlgorithmForAccessToken { get; set; }
    public required string AccessTokenSecretKey { get; set; }
    public required int AccessTokenExpiresMinutes { get; set; }
    
    public required string AlgorithmForRefreshToken { get; set; }
    public required string RefreshTokenSecretKey { get; set; }
    public required int RefreshTokenExpiresDays { get; set; }
}