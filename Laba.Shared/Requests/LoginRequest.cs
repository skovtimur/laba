using System.ComponentModel.DataAnnotations;
using Laba.Shared.Domain.Validators;

namespace Laba.Shared.Requests;

public class LoginRequest
{
    [Required, StringLength(maximumLength: 50, MinimumLength = 5)]
    public required string Email { get; set; }

    [Required, RegularExpression(StrongPasswordValidator.StrongPasswordPattern)]
    public required string Password { get; set; }
}