namespace IdentityService.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}