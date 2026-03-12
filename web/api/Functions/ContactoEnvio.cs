using EnvioMailFrontEnd.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace EnvioMailFrontEnd.Functions
{
    public class ContactoEnvio
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly LandingOptions _landing_options;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ContactoEnvio(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory, IOptions<LandingOptions> landingOptions)
        {
            _logger = loggerFactory.CreateLogger<ContactoEnvio>();
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _landing_options = landingOptions.Value;

        }

        [Function("ContactoEnvio")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ContactoEnvio")]
            HttpRequestData req)
        {
            _logger.LogInformation("ContactoEnvio started.");


            try
            {
                var body = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(req.Body, JsonOptions);
                if (body == null || string.IsNullOrWhiteSpace(body.Token))
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                GoogleCaptchaResponse? captchaResponse = await CheckGoogleCaptcha(body.Token);

                if (captchaResponse?.Success != true)
                {
                    var errors = captchaResponse?.ErrorCodes != null
                        ? string.Join(", ", captchaResponse.ErrorCodes)
                        : "Captcha verification failed";

                    var badCaptcha = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badCaptcha.WriteAsJsonAsync(new { ok = false, error = errors, status = HttpStatusCode.BadRequest });
                    return badCaptcha;
                }

                var functionKey = _configuration["BackEndKey"];

                // Crear cliente y enviar POST a FunctionBackEnd
                var client = _httpClientFactory.CreateClient("FunctionBackEnd");

                client.DefaultRequestHeaders.Remove("x-functions-key");
                client.DefaultRequestHeaders.Add("x-functions-key", functionKey);

                var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                using var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage backendResponse = await client.PostAsync("/api/EnviarMail", httpContent);

                var content = await backendResponse.Content.ReadAsStringAsync();

                object responseBody = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(content) ? "null" : content, JsonOptions);

                var result = new
                {
                    ok = backendResponse.IsSuccessStatusCode,
                    statusCode = (int)backendResponse.StatusCode,
                    response = responseBody
                };
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error EnvioMail.\nMensaje: {ex.Message}\n{ex.StackTrace}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { ok = false, error = ex.Message, status = HttpStatusCode.InternalServerError });
                return errorResponse;
            }
        }

        private async Task<GoogleCaptchaResponse?> CheckGoogleCaptcha(string token)
        {
            var values = new Dictionary<string, string>
            {
                { "secret", _landing_options.SecretKey },
                { "response", token }
            };

            var httpClient = _httpClientFactory.CreateClient("GoogleCaptcha");
            var captchaHttpResponse = await httpClient.PostAsync(
                _landing_options.UrlVerify,
                new FormUrlEncodedContent(values));

            var jsonCaptcha = await captchaHttpResponse.Content.ReadAsStringAsync();
            GoogleCaptchaResponse? captchaResponse = JsonSerializer.Deserialize<GoogleCaptchaResponse>(jsonCaptcha, JsonOptions);

            return captchaResponse;
        }

    }
}