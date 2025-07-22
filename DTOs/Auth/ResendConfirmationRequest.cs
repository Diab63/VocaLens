using System.ComponentModel.DataAnnotations;

namespace VocaLens.DTOs.Auth;

public class ResendConfirmationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
