using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace api;


public class BlobStorage
{
    private readonly ILogger<BlobStorage> _logger;
    private readonly IConfiguration _configuration;
    public BlobStorage(ILogger<BlobStorage> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Function("BlobStorage")]
    public async Task<HttpResponseData> Blob(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var requestJson = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(
            req.Body, JsonOptions);

        // TODO : A futuro, ver de obtener la cadena de conexión desde Key Vault. Por ahora, se obtiene desde las variables de entorno.
        var connString = _configuration["AZURE_STORAGE_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(connString))
        {
            var bad = req.CreateResponse(HttpStatusCode.InternalServerError);
            await bad.WriteAsJsonAsync(new { success = false, error = "Missing storage connection string." });
            return bad;
        }

        try
        {
            var blobServiceClient = new BlobServiceClient(connString);
            var containerClient = blobServiceClient.GetBlobContainerClient("formularios");
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient($"form-{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")}.txt");

            var contenido = $"Nombre: {requestJson?.Nombre} \nEmail: {requestJson?.Email}\nEmpresa: {requestJson?.Empresa}\nTelefono: {requestJson?.Telefono}\nMensaje: {requestJson?.Mensaje}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(contenido));

            var uploadResponse = await blobClient.UploadAsync(stream, overwrite: true);

            var result = new
            {
                success = true,
                httpStatus = uploadResponse.GetRawResponse().Status,
                blobUri = blobClient.Uri.ToString(),
                eTag = uploadResponse.Value?.ETag.ToString(),
                lastModified = uploadResponse.Value?.LastModified
            };

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error de conexión al Storage Account");
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteAsJsonAsync(new { success = false, error = ex.Message });
            return error;
        }
    }
}