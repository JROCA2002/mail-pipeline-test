using EnvioMail.Options;
using EnvioMail.Services;
using EnvioMail.Services.Interfaces;
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
        services.Configure<LandingOptions>(context.Configuration.GetSection("Landing"));
        services.Configure<GraphMailOptions>(context.Configuration.GetSection("Graph"));

        // --- Services ---
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMailService, MailService>();
    })
    .Build();

await host.RunAsync();
