using api.Models;
using EnvioMail;
using EnvioMail.Options;
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

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public EnviarMailDesdeLandingPage(ILoggerFactory loggerFactory,
                IOptions<MailServiceOptions> options,
                IOptions<LandingOptions> landing_options)
        {
            _logger = loggerFactory.CreateLogger<EnviarMailDesdeLandingPage>();
            _mail_options = options.Value;
            _landing_options = landing_options.Value;
        }




        [Function("EnviarMailDesdeLandingPage")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "EnviarMailDesdeLandingPage")]
            HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var body = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(req.Body);
            if (body == null || string.IsNullOrWhiteSpace(body.Token))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            string secretKey = _landing_options.SecretKey;
            string apiUrl = _landing_options.UrlVerify;

            string token = body.Token;

            var values = new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", token }
                };

            using var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(apiUrl, content);
            var jsonString = await response.Content.ReadAsStringAsync();


            var captchaResponse = JsonSerializer.Deserialize<GoogleCaptchaResponse>(jsonString, _jsonOptions);



            if (captchaResponse?.Success != true)
            {
                var badCaptcha = req.CreateResponse(HttpStatusCode.BadRequest);
                await badCaptcha.WriteAsJsonAsync(new { error = "Captcha invalido", status = HttpStatusCode.BadRequest });
                return badCaptcha;
            }
           

          

            string mailFrom = _mail_options.MailFrom;

            if (!string.IsNullOrEmpty(body.Email))
            {
                mailFrom = body.Email.Trim();
            }
           

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
                EnableSsl = true, // FIJO DEBIDO A FALLA DE ANALISIS DE SONARQUBE // _mail_options.SmtpClientEnableSSL,
                UseDefaultCredentials = _mail_options.SmtpClientUseDefaultCredentials,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            try
            {
                await smtp.SendMailAsync(mMailMessage);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new { ok = true });
                _logger.LogInformation("EnviarMailDesdeLandingPage finished successfully.");
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
