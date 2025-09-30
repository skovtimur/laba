using FluentValidation;

namespace Laba.Shared.Extensions;

public static class ValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyUsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.NotEmpty().WithMessage("Username should not be empty")
            .NotNull().WithMessage("Username should not be empty")
            .MinimumLength(3).WithMessage("Username should contain at least 3 characters")
            .MaximumLength(25).WithMessage("Username shouldn't be longer than 25 characters");
    }

    public const string EmailPattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

    public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email should not be empty")
            .NotNull().WithMessage("Email should not be empty")
            .MaximumLength(50).WithMessage("Email shouldn't be longer than 50 characters")
            .MinimumLength(5).WithMessage("Email shouldn't be less than 5 characters")
            .Matches(EmailPattern).WithMessage("This isn't an email address");
    }
}