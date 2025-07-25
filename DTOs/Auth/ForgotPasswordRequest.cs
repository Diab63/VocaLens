using System.ComponentModel.DataAnnotations;

namespace VocaLens.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
