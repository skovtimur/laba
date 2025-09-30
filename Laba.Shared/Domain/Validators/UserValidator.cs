using FluentValidation;
using Laba.Shared.Domain.Entities;
using Laba.Shared.Extensions;

namespace Laba.Shared.Domain.Validators;

public class UserValidator : AbstractValidator<UserEntity>
{
    public UserValidator()
    {
        RuleFor(u => u.Email).NotEmpty().NotNull().SetValidator(new EmailValueObjectValidator());
        RuleFor(u => u.HashedPassword).NotEmpty().NotNull();
        RuleFor(u => u.CreatedAt).NotNull();
        RuleFor(u => u.Id).NotNull().NotEqual(Guid.Empty);
        RuleFor(u => u.Username).ApplyUsernameRules();
    }

    private static readonly UserValidator Validator = new();
    public static bool IsValid(UserEntity userEntity) => Validator.Validate(userEntity).IsValid;
}