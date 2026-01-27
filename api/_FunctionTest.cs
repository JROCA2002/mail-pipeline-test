using EnvioMail;
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

namespace api
{
    public class TestFunction
    {

        private readonly LandingOptions _landing_options;
   

        public TestFunction(IOptions<LandingOptions> landing_options)
        {
           
            _landing_options = landing_options.Value;
           
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
        [Function("load_config")]
        public Task<HttpResponseData> Run(
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
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var model = new
            {
                message = "ELECTA STATIC WEB LANDING",
                random = GenerateRandomHex(30),
                captcha_key = _landing_options.GoogleToken,
                build_info = build_info
            };
            response.WriteString(JsonSerializer.Serialize(model));
            return Task.FromResult(response);
        }

    }
}
