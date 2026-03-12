using BackEndEnvioMail.Models;
using BackEndEnvioMail.Options;
using BackEndEnvioMail.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace BackEndEnvioMail.Services
{
    public class MailService : IMailService
    {
        private readonly IAuthService _authService;
        private readonly MailServiceOptions _mail_options;

        public MailService(IAuthService authService, IOptions<MailServiceOptions> mailOptions)
        {
            _authService = authService;
            _mail_options = mailOptions.Value;
        }

        public async Task SendEmailGraphAsync(MailLandingPageRequest mailRequest)
        {
            GraphServiceClient graphServiceClient;

            var credential = _authService.GetTokenCredential();
            graphServiceClient = new GraphServiceClient(credential);

            var cuerpo = BuildCuerpoMail(mailRequest);
            var message = BuildMessageGraph(_mail_options.Subject,
                                cuerpo,
                                _mail_options.MailTo,
                                mailRequest.Email);

            await graphServiceClient.Users[_mail_options.MailFrom]
            .SendMail
            .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            });
        }

        private static Message BuildMessageGraph(string subject, string cuerpo, string mailTo, string replyTo) => new()
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = cuerpo
            },
            ToRecipients = new List<Recipient>
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress { Address = mailTo }
                }
            },
            ReplyTo = new List<Recipient>
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress { Address = replyTo.Trim() }
                }
            }
        };

        private string BuildCuerpoMail(MailLandingPageRequest mailRequest) => $@"
                <h3>Nuevo mensaje desde el formulario de contacto</h3>
                <p><strong>Nombre:</strong> {mailRequest.Nombre}</p>
                <p><strong>Empresa:</strong> {mailRequest.Empresa}</p>
                <p><strong>Email:</strong> {mailRequest.Email}</p>
                <p><strong>Tel&eacute;fono:</strong> {mailRequest.Telefono}</p>
                <p><strong>Mensaje:</strong></p>
                <p>{mailRequest.Mensaje}</p>
        ";

    }
}
