using EnvioMail;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace api
{
    public class EnviarMailDesdeLandingPage
    {
        private readonly ILogger _logger;
        private readonly MailServiceOptions _mail_options;
        private readonly LandingOptions _landing_options;
        private readonly IConfiguration _config;

        public EnviarMailDesdeLandingPage(ILoggerFactory loggerFactory,
                IOptions<MailServiceOptions> options,
                IOptions<LandingOptions> landing_options,
                IConfiguration config)
        {
            _logger = loggerFactory.CreateLogger<EnviarMailDesdeLandingPage>();
            _mail_options = options.Value;
            _landing_options = landing_options.Value;
            _config = config;
        }




        [Function("EnviarMailDesdeLandingPage")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "EnviarMailDesdeLandingPage")]
            HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var body = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(req.Body);

            string secretKey = _landing_options.SecretKey;
            string apiUrl = _landing_options.UrlVerify;

            string token = body?.Token;

            var values = new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", token }
                };

            using var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(apiUrl, content);
            var jsonString = await response.Content.ReadAsStringAsync();

            var captchaResponse = System.Text.Json.JsonSerializer.Deserialize<GoogleCaptchaResponse>(
                                    jsonString,
                                    new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });


            if (!captchaResponse.Success)
            {
                var badCaptcha = req.CreateResponse(HttpStatusCode.BadRequest);
                await badCaptcha.WriteAsJsonAsync(new { error = "Captcha invalido", status = HttpStatusCode.BadRequest });
                return badCaptcha;
            }
            _logger.LogInformation("Captcha Valido");

            if (body == null || string.IsNullOrEmpty(body.Token))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            string mailFrom = _mail_options.MailFrom;

            if (!string.IsNullOrEmpty(body.Email))
            {
                mailFrom = body.Email.Trim();
            }
            _logger.LogInformation($"Mail from: {mailFrom}");

            string cuerpo = $@"
                    <h3>Nuevo mensaje desde el formulario de contacto</h3>
                    <p><strong>Nombre:</strong> {body.Nombre}</p>
                    <p><strong>Empresa:</strong> {body.Empresa}</p>
                    <p><strong>Email:</strong> {body.Email}</p>
                    <p><strong>Tel&eacute;fono:</strong> {body.Telefono}</p>
                    <p><strong>Mensaje:</strong></p>
                    <p>{body.Mensaje}</p>
                ";

            var mMailMessage = new MailMessage
            {
                From = new MailAddress(mailFrom, _mail_options.MailFromTitulo),
                Subject = _mail_options.Subject,
                Body = cuerpo,
                IsBodyHtml = _mail_options.IsBodyHtml,
                Priority = MailPriority.Normal
            };
            string to = _mail_options.MailTo;
            mMailMessage.To.Add(new MailAddress(to));


            var smtp = new SmtpClient(_mail_options.SmtpClient)
            {
                Port = _mail_options.SmtpClientPort,
                EnableSsl = _mail_options.SmtpClientEnableSSL,
                UseDefaultCredentials = _mail_options.SmtpClientUseDefaultCredentials,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            try
            {
                await smtp.SendMailAsync(mMailMessage);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new { ok = true });
                _logger.LogInformation($"Ejecutando {nameof(EnviarMailDesdeLandingPage)} Fin");
                return ok;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new { error = ex.Message, status =  HttpStatusCode.InternalServerError});
                return error;
            }


        }
    }
}
