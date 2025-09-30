using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Laba.API.Abstract.Interfaces.RepositoryInterfaces;
using Laba.API.Abstract.Interfaces.ServiceInterfaces;
using Laba.API.Options;
using Laba.Shared.Domain.Entities;
using Laba.Shared.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Laba.API.Services;

public class JwtService(IOptions<JwtOptions> jwtOptions, ITokenRepository tokenRepository) : IJwtService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<JwtTokens> GenerateNewTokens(Guid userId, string userName)
    {
        var accessToken = GenerateAccessToken(userId, userName);
        var refreshToken = GenerateRefreshToken(userId, userName);

        await SaveToken(userId, refreshToken);

        return new JwtTokens(
            AccessToken: accessToken,
            RefreshToken: refreshToken);
    }

    private async Task SaveToken(Guid userId, string newRefreshToken)
    {
        var exists = await tokenRepository.ExistsByUserId(userId);

        if (exists)
        {
            await tokenRepository.UpdateToken(userId, newRefreshToken);
        }
        else
        {
            await tokenRepository.AddToken(new TokenEntity { RefreshToken = newRefreshToken, UserId = userId });
        }
    }

    private string GenerateAccessToken(Guid userId, string userName)
    {
        var singingCredentials =
            new SigningCredentials(key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.AccessTokenSecretKey)),
                algorithm: _jwt.AlgorithmForAccessToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiresMinutes),
            signingCredentials: singingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(Guid userId, string userName)
    {
        var singingCredentials =
            new SigningCredentials(key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.RefreshTokenSecretKey)),
                algorithm: _jwt.AlgorithmForRefreshToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiresDays),
            signingCredentials: singingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}