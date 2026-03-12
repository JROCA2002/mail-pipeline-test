using EnvioMailFrontEnd.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace EnvioMailFrontEnd.Functions;

public class LoadConfig
{
    private readonly ILogger<LoadConfig> _logger;
    private readonly LandingOptions _landing_options;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LoadConfig(ILogger<LoadConfig> logger,
                IOptions<LandingOptions> landing_options,
                IHttpClientFactory httpClientFactory,
                IConfiguration configuration)
    {
        _logger = logger;
        _landing_options = landing_options.Value;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public static DateTime GetBuildDate(Assembly assembly)
    {
        try
        {
            string filePath = assembly.Location;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return DateTime.MinValue;

            return File.GetLastWriteTime(filePath);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    [Function("load_config")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "load_config")]
            HttpRequestData req)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        // Get version from AssemblyName
        Version version = assembly.GetName().Version ?? new Version(0, 0, 0, 0);

        // Get build date
        DateTime buildDate = GetBuildDate(assembly);

        // Format output
        string build_info = $"Version {version} built on {buildDate:yyyy-MM-dd HH:mm}";

        // Create a response with status code 200 (OK)
        var response = req.CreateResponse(HttpStatusCode.OK);

        var model = new
        {
            message = "AZURE FUNCTION API DE LA WEB STATIC (CAPTCHA)",
            build_info,
            captcha_options = _landing_options
        };

        // Escribimos de forma asíncrona para evitar la excepción de IO síncrono
        await response.WriteAsJsonAsync(model);
        return response;
    }




    [Function("TestIntegration")]
    public async Task<HttpResponseData> TestIntegration(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TestIntegration")]
            HttpRequestData req)
    {
        _logger.LogInformation("TestIntegration started.");

        HttpResponseMessage backendResponse;
        try
        {
            var client = _httpClientFactory.CreateClient("FunctionBackEnd");

            var functionKey = _configuration["BackEndKey"];
            client.DefaultRequestHeaders.Remove("x-functions-key");
            client.DefaultRequestHeaders.Add("x-functions-key", functionKey);

            backendResponse = await client.GetAsync("api/load_config");
            var content = await backendResponse.Content.ReadAsStringAsync();
            object responseBody = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(content) ? "null" : content, JsonOptions);
            var result = new
            {
                statusCode = (int)backendResponse.StatusCode,
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
}