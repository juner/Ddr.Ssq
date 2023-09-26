using System.Linq;
using System.Text;
using Ddr.Ssq.Printing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConsoleApp = Ddr.Ssq.AnalyzeTool.ConsoleApp;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
await CreateDefaultBuilder(args)
    .RunConsoleAppFrameworkAsync<ConsoleApp>(args);

static IHostBuilder CreateDefaultBuilder(string[] args)
    => Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services)
        => services.AddOptions<OutputOptions>()
            .Bind(context.Configuration.GetSection("Output")))
    .ConfigureLogging((context, builder) =>
    {
        builder.ClearProviders();
        var LoggingSection = context.Configuration.GetSection("Logging");
        if (LoggingSection.GetChildren().Any())
        {
            builder.AddConfiguration(LoggingSection);
            if (LoggingSection.GetSection("Console").GetChildren().Any())
                builder.AddConsole();
            if (LoggingSection.GetSection("Debug").GetChildren().Any())
                builder.AddDebug();
        }
    });
