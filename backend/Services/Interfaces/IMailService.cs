using BackEndEnvioMail.Models;

namespace BackEndEnvioMail.Services.Interfaces
{
    public interface IMailService
    {
        Task SendEmailGraphAsync(MailLandingPageRequest mailRequest);
    }
}
