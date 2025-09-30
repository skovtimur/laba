namespace Laba.Shared.Domain.Dtos;

public class UserDtoToView
{
    public required Guid Id { get; init; } = Guid.NewGuid();
    public required string Username { get; init; }

    public required string Email { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}