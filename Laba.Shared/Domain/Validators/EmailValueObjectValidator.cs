using FluentValidation;
using Laba.Shared.Domain.ValueObjects;
using Laba.Shared.Extensions;

namespace Laba.Shared.Domain.Validators;

public class EmailValueObjectValidator : AbstractValidator<EmailValueObject>
{
    public EmailValueObjectValidator()
    {
        RuleFor(x => x.Email).ApplyEmailRules();
    }

    private static readonly EmailValueObjectValidator Validator = new();
    public static bool IsValid(EmailValueObject email) => Validator.Validate(email).IsValid;
    public static bool IsValid(string email) => Validator.Validate(new EmailValueObject { Email = email }).IsValid;
}