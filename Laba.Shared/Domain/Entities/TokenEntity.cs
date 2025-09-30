namespace Laba.Shared.Domain.Entities;

public class TokenEntity
{
    public required Guid UserId { get; init; }
    public required string RefreshToken { get; init; }
}