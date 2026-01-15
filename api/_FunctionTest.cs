using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace api
{
    public class TestFunction
    {
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



        /// <summary>
        /// 
        /// 
        /// PS C:\Users\hpryz\source\repos\codes\ALUAR> az functionapp deployment source config-zip -g electa-codes-test01_group -n electa-mail --src .\function2-00.zip
        /// 
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("TestMessage")]
        public Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TestMessage")]
            HttpRequestData req)
        {
            // Create a response with status code 200 (OK)
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            var model = new
            {
                message = "Welcome to Azure Functions!",
                random = GenerateRandomHex(30),
                version = "1735"
            };
            response.WriteString(JsonSerializer.Serialize(model));
            return Task.FromResult(response);
        }

    }
}
