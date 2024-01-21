using System.Globalization;
using System.Text;
using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SuperGigaToad;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
Console.OutputEncoding = Encoding.UTF8;

await Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) => {
        configuration
            .MinimumLevel.Information()
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] - {SourceContext}: {Message:lj} {NewLine}{Exception}");
    })
    .ConfigureServices(services => {
        var config =
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();

        services.AddSingleton(serviceProvider => {
            DiscordClient dc = new DiscordClient(new DiscordConfiguration
            {
                Token = config["Token"],
                TokenType = TokenType.Bot,
                LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>(),

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            });

            dc.UseLavalink();
            
            dc.AddExtension(new MusicExtension(serviceProvider));
            dc.AddExtension(new LogExtension(serviceProvider));

            return dc;
        });

        services.AddHostedService<DiscordRunner>();
    })
    .Build()
    .RunAsync();
