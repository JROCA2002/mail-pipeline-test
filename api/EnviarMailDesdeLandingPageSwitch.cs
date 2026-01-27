using api.Models;
using Azure.Identity;
using EnvioMail;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace api
{
    public class EnviarMailDesdeLandingPageSwitch
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly MailServiceOptions _mail_options;
        private readonly LandingOptions _landing_options;
        private readonly GraphMailOptions _graph_options;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EnviarMailDesdeLandingPageSwitch(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IOptions<MailServiceOptions> mailOptions,
            IOptions<LandingOptions> landingOptions,
            IOptions<GraphMailOptions> graphOptions)
        {
            _logger = loggerFactory.CreateLogger<EnviarMailDesdeLandingPageSwitch>();
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

            _mail_options = mailOptions.Value;
            _landing_options = landingOptions.Value;
            _graph_options = graphOptions.Value;
        }

        [Function("EnviarMailDesdeLandingPageSwitch")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "EnviarMailDesdeLandingPageSwitch")]
            HttpRequestData req)
        {
            _logger.LogInformation("EnviarMailDesdeLandingPageSwitch started.");

            var body = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(req.Body, JsonOptions);
            if (body == null || string.IsNullOrWhiteSpace(body.Token))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            // ===== CAPTCHA =====
            var values = new Dictionary<string, string>
            {
                { "secret", _landing_options.SecretKey },
                { "response", body.Token }
            };

            var httpClient = _httpClientFactory.CreateClient();
            var captchaHttpResponse = await httpClient.PostAsync(
                _landing_options.UrlVerify,
                new FormUrlEncodedContent(values));

            var jsonCaptcha = await captchaHttpResponse.Content.ReadAsStringAsync();
            var captchaResponse = JsonSerializer.Deserialize<GoogleCaptchaResponse>(jsonCaptcha, JsonOptions);

            if (captchaResponse?.Success != true)
            {
                var badCaptcha = req.CreateResponse(HttpStatusCode.BadRequest);
                await badCaptcha.WriteAsJsonAsync(new { error = "Captcha invalido", status = HttpStatusCode.BadRequest });
                return badCaptcha;
            }

            // ===== CUERPO MAIL =====
            string cuerpo = $@"
                <h3>Nuevo mensaje desde el formulario de contacto</h3>
                <p><strong>Nombre:</strong> {body.Nombre}</p>
                <p><strong>Empresa:</strong> {body.Empresa}</p>
                <p><strong>Email:</strong> {body.Email}</p>
                <p><strong>Tel&eacute;fono:</strong> {body.Telefono}</p>
                <p><strong>Mensaje:</strong></p>
                <p>{body.Mensaje}</p>
            ";

            string provider = (_configuration["MailProvider"] ?? "SMTP").Trim().ToUpperInvariant();

            try
            {
                if (provider == "GRAPH")
                {
                    // ===== GRAPH =====
                    if (string.IsNullOrWhiteSpace(_graph_options.TenantId) ||
                        string.IsNullOrWhiteSpace(_graph_options.ClientId) ||
                        string.IsNullOrWhiteSpace(_graph_options.ClientSecret) ||
                        string.IsNullOrWhiteSpace(_graph_options.SenderUser))
                    {
                        var bad = req.CreateResponse(HttpStatusCode.InternalServerError);
                        await bad.WriteAsJsonAsync(new
                        {
                            error = "Graph options incompletas (TenantId/ClientId/ClientSecret/SenderUser).",
                            status = HttpStatusCode.InternalServerError
                        });
                        return bad;
                    }

                    var message = new Message
                    {
                        Subject = _mail_options.Subject,
                        Body = new ItemBody
                        {
                            ContentType = BodyType.Html,
                            Content = cuerpo
                        },
                        ToRecipients = new List<Recipient>
                        {
                            new Recipient
                            {
                                EmailAddress = new EmailAddress { Address = _mail_options.MailTo }
                            }
                        }
                    };

                    // ReplyTo (para que al responder vaya al mail del usuario)
                    if (!string.IsNullOrWhiteSpace(body.Email))
                    {
                        message.ReplyTo = new List<Recipient>
                        {
                            new Recipient
                            {
                                EmailAddress = new EmailAddress { Address = body.Email.Trim() }
                            }
                        };
                    }

                    var credential = new ClientSecretCredential(
                        _graph_options.TenantId,
                        _graph_options.ClientId,
                        _graph_options.ClientSecret);

                    var graphClient = new GraphServiceClient(credential);

                    await graphClient.Users[_graph_options.SenderUser]
                        .SendMail
                        .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                        {
                            Message = message,
                            SaveToSentItems = true
                        });

                    var ok = req.CreateResponse(HttpStatusCode.OK);
                    await ok.WriteAsJsonAsync(new { ok = true });
                    _logger.LogInformation("Mail sent successfully via GRAPH.");
                    return ok;
                }

                // ===== SMTP (default) =====
                string mailFrom = _mail_options.MailFrom;
                if (!string.IsNullOrWhiteSpace(body.Email))
                    mailFrom = body.Email.Trim();

                using var mMailMessage = new MailMessage
                {
                    From = new MailAddress(mailFrom, _mail_options.MailFromTitulo),
                    Subject = _mail_options.Subject,
                    Body = cuerpo,
                    IsBodyHtml = _mail_options.IsBodyHtml,
                    Priority = MailPriority.Normal
                };

                mMailMessage.To.Add(new MailAddress(_mail_options.MailTo));

                using var smtp = new SmtpClient(_mail_options.SmtpClient)
                {
                    Port = _mail_options.SmtpClientPort,
                    EnableSsl = _mail_options.SmtpClientEnableSSL,
                    UseDefaultCredentials = _mail_options.SmtpClientUseDefaultCredentials,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                await smtp.SendMailAsync(mMailMessage);

                var okSmtp = req.CreateResponse(HttpStatusCode.OK);
                await okSmtp.WriteAsJsonAsync(new { ok = true });
                _logger.LogInformation("Mail sent successfully via SMTP.");
                return okSmtp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo (provider={Provider})", provider);
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new { error = ex.Message, status = HttpStatusCode.InternalServerError });
                return error;
            }
        }
    }
}
