using EnvioMail.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        // --- Options ---
        services.Configure<LandingOptions>(context.Configuration.GetSection("Landing"));

        // --- HttpClients ---
        services.AddHttpClient("GoogleCaptcha", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddHttpClient("FunctionGraph", client =>
        {
            client.BaseAddress = new Uri(context.Configuration["AzureFunctionGraphURL"] ?? string.Empty);
            //client.Timeout = TimeSpan.FromSeconds(10);
        }); 
    })
    .Build();

await host.RunAsync();