using api.Models;

namespace EnvioMail.Services.Interfaces
{
    public interface IMailService
    {
        Task SendEmailGraphAsync(MailLandingPageRequest mailRequest);
        Task SendEmailSmtpAsync(MailLandingPageRequest mailRequest);
    }
}
