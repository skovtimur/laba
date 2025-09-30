using Laba.Shared.Domain.Validators;
using Laba.Shared.Extensions;

namespace Laba.WebClient.Models.Validators;

public class RegistrationValidator : BaseValidator<RegistrationModel>
{
    public RegistrationValidator()
    {
        RuleFor(u => u.Username).ApplyUsernameRules();
        RuleFor(u => u.Email).ApplyEmailRules();
        RuleFor(u => u.Password).SetValidator(new StrongPasswordValidator());
        RuleFor(u => u.ConfirmPassword).SetValidator(new StrongPasswordValidator());
    }
}