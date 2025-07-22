using System.ComponentModel.DataAnnotations;

namespace VocaLens.DTOs.Auth;

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
