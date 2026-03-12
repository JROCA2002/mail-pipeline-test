using BackEndEnvioMail.Options;
using BackEndEnvioMail.Services;
using BackEndEnvioMail.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // --- Options ---
        services.Configure<MailServiceOptions>(context.Configuration.GetSection("MailService"));

        // --- Services ---
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMailService, MailService>();
    })
    .Build();

await host.RunAsync();
