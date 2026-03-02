using api.Models;
using Azure.Identity;
using EnvioMail.Options;
using EnvioMail.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Abstractions;
using System.Net;
using System.Net.Mail;

namespace EnvioMail.Services
{
    public class MailService : IMailService
    {
        private readonly IAuthService _authService;
        private readonly GraphMailOptions _graph_options;
        private readonly MailServiceOptions _mail_options;
        public MailService( IAuthService authService, IOptions<GraphMailOptions> graphOptions, IOptions<MailServiceOptions> mailOptions)
        {
            _authService = authService;
            _graph_options = graphOptions.Value;
            _mail_options = mailOptions.Value;
        }

        public async Task SendEmailGraphAsync(MailLandingPageRequest mailRequest)
        {
            GraphServiceClient graphServiceClient;

            try
            {
                var credential = _authService.GetTokenCredential();
                graphServiceClient = new GraphServiceClient(credential);

                var cuerpo = BuildCuerpoMail(mailRequest);
                var message = BuildMessageGraph(_mail_options.Subject, cuerpo, _mail_options.MailTo, mailRequest.Email);

                await graphServiceClient.Users[_graph_options.SenderUser]
                .SendMail
                .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                    {
                        Message = message,
                        SaveToSentItems = true
                    }
                );
            }
            catch (AuthenticationFailedException ex)
            {
                throw;
            }
            catch (ApiException apiEx)
            {
                throw;    
            }
        }

        public async Task SendEmailSmtpAsync(MailLandingPageRequest mailRequest)
        {
            string mailFrom = _mail_options.MailFrom;

            string cuerpo = BuildCuerpoMail(mailRequest);

            using var mMailMessage = new MailMessage
            {
                From = new MailAddress(mailFrom, _mail_options.MailFromTitulo),
                Subject = _mail_options.Subject,
                Body = cuerpo,
                IsBodyHtml = _mail_options.IsBodyHtml,
                Priority = MailPriority.Normal
            };

            mMailMessage.To.Add(new MailAddress(_mail_options.MailTo));

            // Reply-To para que el destinatario responda al usuario
            if (!string.IsNullOrWhiteSpace(mailRequest.Email))
            {
                mMailMessage.ReplyToList.Add(new MailAddress(mailRequest.Email.Trim()));
            }

            try
            {
                using var smtp = new SmtpClient(_mail_options.SmtpClient)
                {
                    Port = _mail_options.SmtpClientPort,
                    EnableSsl = true, // FIJO POR SONARQUBE
                    UseDefaultCredentials = _mail_options.SmtpClientUseDefaultCredentials,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                await smtp.SendMailAsync(mMailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Métodos Privados
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

        #endregion
    }
}
