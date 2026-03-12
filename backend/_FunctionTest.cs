using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using EnvioMail.Options;
using EnvioMail.Services.Interfaces;
using System.Threading;

namespace api
{
    public class TestFunction
    {
        private readonly LandingOptions _landing_options;
        private readonly MailServiceOptions _mail_options;
        private readonly GraphMailOptions _graph_options;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public TestFunction(IConfiguration configuration,
            IOptions<LandingOptions> landing_options,
            IOptions<MailServiceOptions> mailOptions,
            IOptions<GraphMailOptions> graphOptions,
            IAuthService authService)
        {
            _configuration = configuration;
            _landing_options = landing_options.Value;
            _mail_options = mailOptions.Value;
            _graph_options = graphOptions.Value;
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

        /// <summary>
        /// 
        /// 
        /// PS C:\Users\hpryz\source\repos\codes\ALUAR> az functionapp deployment source config-zip -g electa-codes-test01_group -n electa-mail --src .\function2-00.zip
        /// 
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// 
        // TODO: Este método es de prueba, luego de las pruebas esto se elimina.

        [Function("load_config")]
        public Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "load_config")]
            HttpRequestData req)
        {

            string provider = (_configuration["MailProvider"] ?? "SMTP").Trim().ToUpperInvariant();

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
                build_info = build_info,
                mail_provider = provider,
                mail_options = new
                {
                    MailFrom = _mail_options.MailFrom,
                    MailTo = _mail_options.MailTo,
                },
                _graph_options = _graph_options
            };
            response.WriteString(JsonSerializer.Serialize(model));
            return Task.FromResult(response);
        }


        // TODO: Este método es de prueba, luego de las pruebas esto se elimina.
        [Function("status")]
        public Task<HttpResponseData> Status(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")]
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


        // TODO: Este método es de prueba, luego de las pruebas esto se elimina.
        [Function("graph_auth")]
        public async Task<HttpResponseData> GraphAuth(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "graph_auth")]
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
                if (!string.IsNullOrWhiteSpace(_graph_options.SenderUser))
                {
                    try
                    {
                        var graphClient = new GraphServiceClient(credential);
                        var user = await graphClient.Users[_graph_options.SenderUser].GetAsync();
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
                    senderUser = _graph_options.SenderUser,
                    token = accessToken.Token,
                    user = userInfo
                });
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch (AuthenticationFailedException ex)
            {
                await response.WriteAsJsonAsync(new { success = false, error = "Authentication failed", message = ex.Message , detail = ex.InnerException?.Message});
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
