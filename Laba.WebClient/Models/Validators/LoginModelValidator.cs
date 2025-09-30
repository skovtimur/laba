using Laba.Shared.Domain.Validators;
using Laba.Shared.Extensions;

namespace Laba.WebClient.Models.Validators;

public class LoginModelValidator : BaseValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(u => u.Email).ApplyEmailRules();
        RuleFor(u => u.Password).SetValidator(new StrongPasswordValidator());
    }
}