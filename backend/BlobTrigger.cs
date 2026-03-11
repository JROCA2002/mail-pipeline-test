using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using api.Models;
using EnvioMail.Services.Interfaces;
using System.Text.Json;

namespace EnvioMail;

public class BlobTrigger
{
    private readonly ILogger<BlobTrigger> _logger;
    private readonly IMailService _mailService;

    public BlobTrigger(ILogger<BlobTrigger> logger, IMailService mailService)
    {
        _logger = logger;
        _mailService = mailService;
    }

    [Function(nameof(BlobTrigger))]
    public async Task Run([BlobTrigger("formularios/{name}.txt", Connection = "AzureWebJobsStorage")] Stream stream, string name)
    {
        _logger.LogInformation($"Archivo detectado en blob: {name}");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var contenido = await reader.ReadToEndAsync();

        // Parseá línea por línea
        var lineas = contenido.Split('\n');
        var datos = lineas
            .Where(l => l.Contains(':'))
            .ToDictionary(
                l => l.Split(':')[0].Trim(),
                l => string.Join(":", l.Split(':').Skip(1)).Trim()
            );

        var nombre = datos.GetValueOrDefault("Nombre");
        var email = datos.GetValueOrDefault("Email");
        var mensaje = datos.GetValueOrDefault("Mensaje");
        var telefono = datos.GetValueOrDefault("Telefono");
        var empresa = datos.GetValueOrDefault("Empresa");

        _logger.LogInformation($"Contenido del archivo: {contenido}");
        _logger.LogInformation("C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}", name, contenido);

        try
        {
            var mailRequest = new MailLandingPageRequest()
            {
                Nombre = nombre,
                Email = email,
                Mensaje = mensaje,
                Telefono = telefono,
                Empresa = empresa
            };
            if (mailRequest == null)
            {
                _logger.LogWarning("Contenido del blob no pudo deserializarse a MailLandingPageRequest.");
                return;
            }

            await _mailService.SendEmailSmtpAsync(mailRequest);
            _logger.LogInformation("Correo enviado correctamente desde BlobTrigger para: {email}", mailRequest.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando blob {name} o enviando correo.", name);
        }
    }
}