using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VocaLens.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; } 
    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public string PasswordResetOtp { get; set; } = string.Empty;
    public DateTime? PasswordResetExpiresAt { get; set; } 

    public bool IsOtpVerified { get; set; }
}
