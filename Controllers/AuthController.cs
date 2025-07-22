using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VocaLens.DTOs.Auth;
using VocaLens.Entities;
using VocaLens.Services;
using VocaLens.Data;

namespace VocaLens.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly EmailService _emailService;
    private readonly ApplicationDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService,
        EmailService emailService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Generate OTP
        var otp = GenerateOtp();
        user.OtpCode = otp;
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5); // OTP valid for 5 minutes
        await _userManager.UpdateAsync(user);

        // Send OTP via email
        await _emailService.SendConfirmationEmailAsync(user.Email!, user.FirstName ?? "User", otp);

        return Ok(new { message = "Registration successful. Please check your email for the confirmation code." });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return NotFound("User not found.");

        if (user.OtpCode != request.OtpCode)
            return BadRequest("Invalid confirmation code.");

        if (user.OtpExpiresAt < DateTime.UtcNow)
            return BadRequest("Confirmation code has expired.");

        user.EmailConfirmed = true;
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Email confirmed successfully." });
    }

    //[HttpPost("resend-confirmation")]
    //public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequest request)
    //{
    //    var user = await _userManager.FindByEmailAsync(request.Email);
    //    if (user == null)
    //        return NotFound("User not found.");

    //    if (user.EmailConfirmed)
    //        return BadRequest("Email is already confirmed.");

    //    // Generate new OTP
    //    var otp = GenerateOtp();
    //    user.OtpCode = otp;
    //    user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(15);
    //    await _userManager.UpdateAsync(user);

    //    // Send new OTP via email
    //    await _emailService.SendEmailAsync(user.Email!, "Confirm your email",
    //        $"Your new confirmation code is: {otp}. This code will expire in 15 minutes.");

    //    return Ok(new { message = "New confirmation code sent. Please check your email." });
    //}

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return NotFound("User not found.");

        if (user.EmailConfirmed)
            return BadRequest("Email is already confirmed.");

        // Generate new OTP
        var otp = GenerateOtp();
        user.OtpCode = otp;
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        await _userManager.UpdateAsync(user);

        // Send email with template
        await _emailService.SendConfirmationEmailAsync(user.Email!, user.FirstName ?? "User", otp);

        return Ok(new { message = "New confirmation code sent. Please check your email." });
    }


    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        if (!await _userManager.IsEmailConfirmedAsync(user))
            return Unauthorized(new { message = "Please confirm your email before logging in." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password" });

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(IpAddress());

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(2880)
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid request.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest("Invalid request.");

        // ✅ Ensure OTP was verified
        if (!user.IsOtpVerified)
            return BadRequest("OTP verification required before resetting password.");

        // ✅ Reset the password
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // ✅ Clear OTP and verification status after successful reset
        user.PasswordResetOtp = string.Empty;
        user.IsOtpVerified = false;
        user.PasswordResetExpiresAt = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Password reset successfully." });
    }


    [HttpPost("verify-resetpass-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (request == null || !ModelState.IsValid)
            return BadRequest("Invalid request.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest("Invalid request.");

        // ✅ Check OTP validity
        if (string.IsNullOrEmpty(user.PasswordResetOtp) ||
            user.PasswordResetExpiresAt < DateTime.UtcNow ||
            user.PasswordResetOtp != request.PasswordResetOtp)
        {
            return BadRequest("Invalid or expired reset code.");
        }

        // ✅ Mark OTP as verified
        user.IsOtpVerified = true;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "OTP verified successfully. You can now reset your password." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordRequest request)
    {

        if (!ModelState.IsValid)
            return BadRequest("Invalid email address.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest("No user found with this email.");
        if (!user.EmailConfirmed)
            return BadRequest("Please confirm your email before resetting your password.");
        // ✅ Generate OTP
        var otp = GenerateOtp();
        user.PasswordResetOtp = otp;
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddMinutes(5); // OTP valid for 5 minutes
        user.IsOtpVerified = false; // Ensure fresh verification is needed
        await _userManager.UpdateAsync(user);

        // ✅ Send OTP via email
        await _emailService.SendResetEmailAsync(user.Email!, user.FirstName ?? "User", otp);

        return Ok(new { message = "OTP sent successfully. Please check your email." });
    }

    [HttpPost("resend-reset-password")]
    public async Task<IActionResult> ResendResetPasswordEmail([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid email address.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return NotFound("User not found.");
        if (!user.EmailConfirmed)
            return BadRequest("Please confirm your email before resetting your password.");

        // Generate new OTP
        var otp = GenerateOtp();
        user.PasswordResetOtp = otp;
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddMinutes(5);
        await _userManager.UpdateAsync(user);

        // Send new OTP via email
        await _emailService.SendResetEmailAsync(user.Email!, user.FirstName ?? "User", otp);

        return Ok(new { message = "New password reset code sent. Please check your email." });
    }


    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == request.RefreshToken));

        if (user == null)
            return Unauthorized(new { message = "Invalid refresh token" });

        var oldRefreshToken = user.RefreshTokens.Single(t => t.Token == request.RefreshToken);

        if (!oldRefreshToken.IsActive)
            return Unauthorized(new { message = "Invalid refresh token" });

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken(IpAddress());

        // Revoke the old refresh token
        oldRefreshToken.RevokedDate = DateTime.UtcNow;
        oldRefreshToken.RevokedByIp = IpAddress();
        oldRefreshToken.ReplacedByToken = newRefreshToken.Token;

        // Add the new refresh token
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15)
        });
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken(RefreshTokenRequest request)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == request.RefreshToken));

        if (user == null)
            return NotFound(new { message = "Invalid refresh token" });

        var token = user.RefreshTokens.Single(t => t.Token == request.RefreshToken);

        if (!token.IsActive)
            return NotFound(new { message = "Invalid refresh token" });

        // Revoke token
        token.RevokedDate = DateTime.UtcNow;
        token.RevokedByIp = IpAddress();
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Token revoked" });
    }

    private string IpAddress()
    {
        return Request.Headers.TryGetValue("X-Forwarded-For", out var ipAddress)
            ? ipAddress!
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }

    private string GenerateOtp()
    {
        // Generate a 6-digit OTP
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }

    [HttpGet("GetAccessToken")]
    public async Task<IActionResult> GetAccessToken()
    {
            var User = (await _context.RefreshTokens.OrderBy(r => r.CreatedDate).Include(r => r.User).LastOrDefaultAsync())?.User;
            if(User is null)
                return NotFound(new { Message = "No user has logged in" });
            var AccessToken = _jwtService.GenerateAccessToken(User);
            return Ok(AccessToken);
    }

}



