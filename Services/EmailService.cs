using VocaLens.DTOs.Settings;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

public class EmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        message.To.Add(to);

        using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
            EnableSsl = _emailSettings.EnableSsl
        };

        await client.SendMailAsync(message);
    }

    public async Task SendConfirmationEmailAsync(string email, string name, string otp)
    {
        string templatePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "Confirmation Template.html");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found at {templatePath}.");
        }

        string body = await File.ReadAllTextAsync(templatePath);
        body = body.Replace("{{name}}", name)
                   .Replace("{{otp}}", otp);

        await SendEmailAsync(email, "Confirm Your Email", body, true);
    }

    public async Task SendResetEmailAsync(string email, string name, string otp)
    {
        string templatePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "ResetPass Template.html");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found at {templatePath}.");
        }

        string body = await File.ReadAllTextAsync(templatePath);
        body = body.Replace("{{name}}", name)
                   .Replace("{{otp}}", otp);

        await SendEmailAsync(email, "Reset your password", body, true);
    }


}
