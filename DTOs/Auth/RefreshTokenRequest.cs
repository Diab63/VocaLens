using System.ComponentModel.DataAnnotations;

namespace VocaLens.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
