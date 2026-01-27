using EnvioMail;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<MailServiceOptions>(context.Configuration.GetSection("MailService"));
        services.Configure<LandingOptions>(context.Configuration.GetSection("Landing"));
    })
    .Build();

await host.RunAsync();
