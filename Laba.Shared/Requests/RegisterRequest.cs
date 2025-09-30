using System.ComponentModel.DataAnnotations;
using Laba.Shared.Domain.Validators;
using Laba.Shared.Extensions;

namespace Laba.Shared.Requests;

public class RegisterRequest
{
    [Required, StringLength(maximumLength: 25, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required, StringLength(maximumLength: 50, MinimumLength = 5), RegularExpression(ValidationRules.EmailPattern)]
    public required string Email { get; set; }

    [Required, RegularExpression(StrongPasswordValidator.StrongPasswordPattern)]
    public required string Password { get; set; }
}