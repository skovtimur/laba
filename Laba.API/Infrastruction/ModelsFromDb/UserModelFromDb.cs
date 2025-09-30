namespace Laba.API.Infrastruction.ModelsFromDb;

public class UserModelFromDb
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }

    public required string Email { get; init; }
    public required string HashedPassword { get; init; }
    public required DateTime CreatedAt { get; init; }
}