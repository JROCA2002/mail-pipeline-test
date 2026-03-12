using EnvioMail.Models;

namespace EnvioMail.Services.Interfaces
{
    public interface IMailService
    {
        Task SendEmailGraphAsync(MailLandingPageRequest mailRequest);
    }
}
