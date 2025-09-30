using Laba.Shared.Domain.Validators;
using Laba.Shared.Domain.ValueObjects;

namespace Laba.Shared.Domain.Entities;

public class UserEntity
{
    public required Guid Id { get; init; } = Guid.NewGuid();
    public required string Username { get; init; }

    public required EmailValueObject Email { get; init; }
    public required HashedPasswordValueObject HashedPassword { get; init; }
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public static UserEntity? Create(string username, EmailValueObject email,
        HashedPasswordValueObject hashedPassword)
    {
        var user = new UserEntity
        {
            Username = username,
            Email = email,
            HashedPassword = hashedPassword,
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        return UserValidator.IsValid(user) ? user : null;
    }
}