using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Players.Configuration;
using Samples.Players.DataAccess;
using Samples.Players.Middleware;
using Samples.Players.Services;

namespace Samples.Players;

internal static class Program
{
    internal static async Task Main()
    {
        var urls = BuildConfiguration.GetValue("SamplesPlayers:httphosturl", $"http://*:{8082}");

        var port = urls.LastRightPart(':').ToInt(8082);

        var builder = new HostBuilder().UseContentRoot(Directory.GetCurrentDirectory())
                                       .ConfigureHostConfiguration(b => b.AddConfiguration(BuildConfiguration))
                                       .ConfigureAppConfiguration((_, conf) => conf.AddConfiguration(BuildConfiguration))
                                       .ConfigureLogging((x, b) => b.AddConfiguration(x.Configuration.GetSection("Logging"))
                                                                    .AddSimpleConsole(o =>
                                                                                      {
                                                                                          o.SingleLine = true;
                                                                                          o.TimestampFormat = "HH:mm:ss.fff ";
                                                                                      }))
                                       .ConfigureWebHost(whb => whb.UseShutdownTimeout(TimeSpan.FromSeconds(15))
                                                                   .UseKestrel(ko =>
                                                                               {
                                                                                   ko.Listen(IPAddress.Any, port, l => l.Protocols = HttpProtocols.Http1AndHttp2);
                                                                                   ko.Limits.MaxRequestBodySize = 2000 * 1024;
                                                                                   ko.AllowSynchronousIO = false;
                                                                               })
                                                                   .UseStartup<SampleStartup>())
                                       .UseConsoleLifetime();

        var host = builder.Build();

        // Migrate/setup data stores
        var migrated = await host.Services.MigrateDataStoresAsync();

        // Background some seed data
        if (migrated)
        {
            var demoDataService = host.Services.GetRequiredService<IDemoDataService>();
#pragma warning disable 4014
            demoDataService.CreateDemoDataAsync();
#pragma warning restore 4014
        }

        await host.RunAsync();
    }

    private static IConfiguration BuildConfiguration { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.Global.json", true)
                                                                                          .AddJsonFile("appsettings.json", true)
                                                                                          .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                                                                                          .AddEnvironmentVariables("PLAYERS_")
                                                                                          .Build();
}

internal class SampleStartup
{
    public void Configure(IApplicationBuilder app)
    {
        app.RegisterOnApplicationStopping(() =>
                                          {
                                              var logger = app.ApplicationServices.GetRequiredService<ILogger<SampleStartup>>();

                                              SampleShutdownCancellationSource.Instance.TryCancel();

                                              logger.LogInformation("*** Shutdown initiated, stopping services...");
                                          })
           .RegisterOnApplicationStopped(() =>
                                         {
                                             var logger = app.ApplicationServices.GetRequiredService<ILogger<SampleStartup>>();

                                             logger.LogInformation("*** Shutdown completed, exiting...");
                                         })
           .RegisterOnApplicationStarted(() =>
                                         {
                                             var logger = app.ApplicationServices.GetRequiredService<ILogger<SampleStartup>>();

                                             logger.LogInformation("SamplePlayer service is ready for requests");
                                         })
           .UseRouting()
           .UseMiddleware<SampleLogMiddleware>()
           .UseHealthChecks("/ping")
           .UseEndpoints(r => { r.MapControllers(); });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks()
                .Add(new HealthCheckRegistration("SamplePlayerService", ServicePingHealthCheck.Instance, HealthStatus.Unhealthy,
                                                 Enumerable.Empty<string>(), TimeSpan.FromSeconds(7)));

        services.AddControllers()
                .AddJsonOptions(x =>
                                {
                                    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                                    x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                                    x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                                    x.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                                    x.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                                    x.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                                })
                .AddMvcOptions(o =>
                               {
                                   o.Filters.Add(new ModelAttributeValidationFilter());
                                   o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "The field is required.");
                                   o.MaxModelValidationErrors = 10;
                               });

        services.AddElasticPlayerStorage();

        services.AddSingleton<IDemoDataService, CbsDemoDataService>()
                .AddSingleton<INameBriefTransformer, StaticNameBriefTransformer>();
    }
}
