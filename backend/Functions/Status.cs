using Azure.Core;
using Azure.Identity;
using BackEndEnvioMail.Options;
using BackEndEnvioMail.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BackEndEnvioMail.Functions
{
    public class TestFunction
    {
        private readonly MailServiceOptions _mail_options;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public TestFunction(IConfiguration configuration,
            IOptions<MailServiceOptions> mailOptions,
            IAuthService authService)
        {
            _configuration = configuration;
            _mail_options = mailOptions.Value;
            _authService = authService;
        }

        public static string GenerateRandomHex(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

            // Each byte = 2 hex characters
            int byteCount = (length + 1) / 2;
            byte[] randomBytes = new byte[byteCount];

            // Fill with cryptographically secure random bytes
            RandomNumberGenerator.Fill(randomBytes);

            // Convert bytes to hex string
            StringBuilder sb = new StringBuilder(byteCount * 2);
            foreach (byte b in randomBytes)
                sb.Append(b.ToString("X2")); // Uppercase hex

            // Trim to requested length
            return sb.ToString(0, length);
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
        public Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "load_config")]
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
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var model = new
            {
                source = "AZURE FUNCTION MICROSOFT GRAPH (BACKEND)",
                random = GenerateRandomHex(30),
                build_info,
                mail_options = _mail_options
            };
            response.WriteString(JsonSerializer.Serialize(model));
            return Task.FromResult(response);
        }


        [Function("status")]
        public Task<HttpResponseData> Status(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")]
            HttpRequestData req)
        {
            // Enumerate functions in the current assembly and try to extract routes when present
            var assembly = Assembly.GetExecutingAssembly();
            var endpoints = new List<object>();

            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    var funcAttr = method.GetCustomAttribute<FunctionAttribute>();
                    if (funcAttr != null)
                    {
                        string functionName = funcAttr.Name ?? method.Name;
                        string route = "";

                        var parameters = method.GetParameters();
                        foreach (var p in parameters)
                        {
                            var httpAttr = p.GetCustomAttribute<HttpTriggerAttribute>();
                            if (httpAttr != null)
                            {
                                route = httpAttr.Route ?? "";
                                break;
                            }
                        }

                        endpoints.Add(new
                        {
                            function = functionName,
                            route = string.IsNullOrEmpty(route) ? "(no route / default)" : route
                        });
                    }
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var model = new
            {
                implemented = true,
                message = "Function app is implemented",
                endpoints = endpoints.DistinctBy(e => (e as dynamic).function).ToList()
            };

            response.WriteString(JsonSerializer.Serialize(model));
            return Task.FromResult(response);
        }


        [Function("graph_auth")]
        public async Task<HttpResponseData> GraphAuth(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "graph_auth")]
            HttpRequestData req)
        {
            HttpResponseData response = req.CreateResponse();
            try
            {
                // Usar el servicio de autenticación para obtener el TokenCredential
                TokenCredential credential = _authService.GetTokenCredential();

                // Solicitar un AccessToken para Graph
                var tokenRequest = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
                var accessToken = await credential.GetTokenAsync(tokenRequest, CancellationToken.None);

                // Intentar obtener información del usuario configurado (opcional)
                object? userInfo = null;
                if (!string.IsNullOrWhiteSpace(_mail_options.MailFrom))
                {
                    try
                    {
                        var graphClient = new GraphServiceClient(credential);
                        var user = await graphClient.Users[_mail_options.MailFrom].GetAsync();
                        userInfo = new
                        {
                            id = user?.Id,
                            displayName = user?.DisplayName,
                            userPrincipalName = user?.UserPrincipalName
                        };
                    }
                    catch (Exception gx)
                    {
                        userInfo = new { error = "Unable to fetch user", detail = gx.Message };
                    }
                }

                await response.WriteAsJsonAsync(new
                {
                    success = true,
                    message = "Authenticated to Microsoft Graph (token acquired)",
                    expiresOn = accessToken.ExpiresOn,
                    senderUser = _mail_options.MailFrom,
                    token = accessToken.Token,
                    user = userInfo
                });
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch (AuthenticationFailedException ex)
            {
                await response.WriteAsJsonAsync(new { success = false, error = "Authentication failed", message = ex.Message, detail = ex.InnerException?.Message });
                response.StatusCode = HttpStatusCode.Unauthorized;
                return response;
            }
            catch (Exception ex)
            {
                await response.WriteAsJsonAsync(new { success = false, error = "Error acquiring token or calling Graph", detail = ex.Message });
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }

    }
}
