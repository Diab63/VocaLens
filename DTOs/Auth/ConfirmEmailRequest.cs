using System.ComponentModel.DataAnnotations;

namespace VocaLens.DTOs.Auth;

public class ConfirmEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string OtpCode { get; set; } = string.Empty;
}
