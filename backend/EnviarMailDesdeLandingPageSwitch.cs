using api.Models;
using Azure.Core;
using Azure.Identity;
using EnvioMail.Options;
using EnvioMail.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using System.Net;
using System.Text.Json;

namespace EnvioMail
{
    public class EnviarMailDesdeLandingPageSwitch
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EnviarMailDesdeLandingPageSwitch(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMailService mailService)
        {
            _logger = loggerFactory.CreateLogger<EnviarMailDesdeLandingPageSwitch>();
            _configuration = configuration;
            _mailService = mailService;
        }

        [Function("EnviarMailDesdeLandingPageSwitch")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "EnviarMailDesdeLandingPageSwitch")]
            HttpRequestData req)
        {
            _logger.LogInformation("EnviarMailDesdeLandingPageSwitch started.");

            var body = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(req.Body, JsonOptions);
            if (body == null || string.IsNullOrWhiteSpace(body.Token))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            string provider = (_configuration["MailProvider"] ?? "SMTP").Trim().ToUpperInvariant();
            HttpResponseData response = req.CreateResponse();
            try
            {
                if (provider == "SMTP")
                {
                    await _mailService.SendEmailSmtpAsync(body);
                }
                else
                {
                    await _mailService.SendEmailGraphAsync(body);
                }

                await response.WriteAsJsonAsync(new { ok = true , message = $"Mail enviado vía: {provider}"});
                response.StatusCode = HttpStatusCode.OK;

                _logger.LogInformation($"Mail sent successfully via: {provider}.");
                return response;

            }
            catch(AuthenticationFailedException authEx)
            {
                _logger.LogError(authEx, $"Authentication error sending email ({provider}).\nMensaje:{authEx.Message}");
                await response.WriteAsJsonAsync(new { error = authEx.Message, status = HttpStatusCode.Unauthorized, messageGraph = authEx.Message, messageDetail = authEx.InnerException?.Message });
                response.StatusCode = HttpStatusCode.Unauthorized;
                return response;
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, $"Graph API error sending email ({provider}).\nMensaje:{apiEx.Message}");
                await response.WriteAsJsonAsync(new { error = apiEx.Message, status = (HttpStatusCode)apiEx.ResponseStatusCode, messageGraph = apiEx.Message, messageDetail = apiEx.InnerException?.Message });
                response.StatusCode = (HttpStatusCode)apiEx.ResponseStatusCode;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en {provider}.\nMensaje: {ex.Message}");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new { ok = false,  error = ex.Message, status = HttpStatusCode.InternalServerError });
                return error;
            }
        }
    }
}
