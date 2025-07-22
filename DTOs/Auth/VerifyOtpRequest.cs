using System.ComponentModel.DataAnnotations;
namespace VocaLens.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordResetOtp { get; set; } = string.Empty;
    }
}
