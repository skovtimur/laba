using System.ComponentModel.DataAnnotations;

namespace Laba.Shared.Requests;

public class EndHandshakeRequest
{
    [Required, StringLength(maximumLength: 50, MinimumLength = 5)]
    public required string Email { get; set; }

    [Required]
    public required string HashedResponse { get; set; }
}