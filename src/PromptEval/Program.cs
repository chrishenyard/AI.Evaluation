using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using PromptEval;
using PromptEval.Config;
using Serilog;

namespace YourNamespace;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Log.Information("Starting console host");

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile(
                        $"appsettings.{env.EnvironmentName}.json",
                        optional: true,
                        reloadOnChange: true);
                    config.AddUserSecrets<Program>();
                    config.AddEnvironmentVariables();

                    if (args.Length > 0)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext();
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddOptions<KernelSettings>()
                        .Bind(context.Configuration.GetSection("KernelSettings"))
                        .ValidateOnStart();

                    services.AddTransient(sp =>
                    {
                        var kernelSettings = sp
                            .GetRequiredService<IOptions<KernelSettings>>()
                            .Value;

                        var kernelBuilder = Kernel.CreateBuilder();

                        kernelBuilder.Services.AddLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.AddSerilog(dispose: false);
                            logging.SetMinimumLevel(LogLevel.Information).AddDebug();
                        });

                        kernelBuilder.Services.AddChatCompletionService(kernelSettings);

                        return kernelBuilder.Build();
                    });

                    services.AddHostedService<ConsoleChat>();
                })
                .Build();

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Console host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}