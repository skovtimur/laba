using Laba.Shared.Domain.Validators;

namespace Laba.Shared.Domain.ValueObjects;

public struct EmailValueObject
{
    public string Email { get; init; }

    public static EmailValueObject? Create(string email)
    {
        var valueObject = new EmailValueObject { Email = email };
        return EmailValueObjectValidator.IsValid(valueObject) ? valueObject : null;
    }
}