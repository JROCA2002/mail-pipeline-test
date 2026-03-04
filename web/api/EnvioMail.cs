using api.Models;
using EnvioMail.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace EnvioMail
{
    public class EnvioMail
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly LandingOptions _landing_options;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EnvioMail(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory, IOptions<LandingOptions> landingOptions)
        {
            _logger = loggerFactory.CreateLogger<EnvioMail>();
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _landing_options = landingOptions.Value;

        }

        [Function("EnvioMail")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "EnvioMail")]
            HttpRequestData req)
        {
            _logger.LogInformation("EnvioMail started.");

            var body = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(req.Body, JsonOptions);
            if (body == null || string.IsNullOrWhiteSpace(body.Token))
                return req.CreateResponse(HttpStatusCode.BadRequest);

           
       

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


            HttpResponseData response = req.CreateResponse();
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, $"Error en {}.\nMensaje: {ex.Message}");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new { error = ex.Message, status = HttpStatusCode.InternalServerError });
                return error;
            }
        }
    }
}