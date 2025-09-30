using System.Text.RegularExpressions;
using FluentValidation;

namespace Laba.Shared.Domain.Validators;

public class StrongPasswordValidator : AbstractValidator<string>
{
    public StrongPasswordValidator()
    {
        RuleFor(x => x).NotEmpty().WithMessage("The Password shouldn't be empty").
            NotNull().WithMessage("The Password shouldn't be empty")
            .Must(IsStrongPassword).WithMessage("The Password should contain little and big characters and, numbers and special characters");
    }

    public const string StrongPasswordPattern = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^\w\s]).{8,24}$";
    private static readonly Regex StrongPasswordRegex = new(StrongPasswordPattern);
    private static readonly StrongPasswordValidator Validator = new();

    public static bool IsStrongPassword(string password) => StrongPasswordRegex.IsMatch(password);
    public static bool IsValid(string password) => Validator.Validate(password).IsValid;
}