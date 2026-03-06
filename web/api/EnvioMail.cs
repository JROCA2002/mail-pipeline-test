using EnvioMail.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace api
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

            // TODO : Se comentan el checkGoogleCaptcha para poder verificar la seguridad de la funcion. Luego de las pruebas, descomentar.

            /*
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
            */
            try
            {
                var functionKey = _configuration["AzureFunctionGraphKey"];   

                // Crear cliente y enviar POST a FunctionGraph
                var client = _httpClientFactory.CreateClient("FunctionGraph");

                client.DefaultRequestHeaders.Remove("x-functions-key");
                client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
                
                var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                using var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage graphResponse = await client.PostAsync("/api/EnviarMailDesdeLandingPageSwitch", httpContent);

                var content = await graphResponse.Content.ReadAsStringAsync();

                object responseBody = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(content) ? "null" : content, JsonOptions);

                var result = new
                {
                    ok = graphResponse.IsSuccessStatusCode,
                    statusCode = (int)graphResponse.StatusCode,
                    response = responseBody
                };
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, $"Error en {}.\nMensaje: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { ok = false, error = ex.Message, status = HttpStatusCode.InternalServerError });
                return errorResponse;
            }
        }

        // TODO : Este método es de prueba, luego de las pruebas esto se elimina.
        [Function("TestAuth")]
        public async Task<HttpResponseData> TestAuth(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get_auth")]
            HttpRequestData req)
        {
            _logger.LogInformation("TestAuth started.");

            var client = _httpClientFactory.CreateClient("FunctionGraph");

            HttpResponseMessage graphResponse;
            try
            {
                graphResponse = await client.GetAsync("api/graph_auth");
                var content = await graphResponse.Content.ReadAsStringAsync();
                object responseBody = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(content) ? "null" : content, JsonOptions);
                var result = new
                {
                    statusCode = (int)graphResponse.StatusCode,
                    response = responseBody
                };

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(result);
                return ok;
            }
            catch (Exception ex)
            {
                var error = req.CreateResponse(HttpStatusCode.BadGateway);
                await error.WriteAsJsonAsync(new { error = ex.Message, status = (int)HttpStatusCode.BadGateway });
                return error;
            }

        }

        #region Métodos privados
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
        #endregion
    }
}