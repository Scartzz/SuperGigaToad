using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Hosting;

namespace SuperGigaToad;

public class DiscordRunner : IHostedService
{
    private readonly DiscordClient _discordClient;
    
    public DiscordRunner(DiscordClient discordClient)
    {
        this._discordClient = discordClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var endpoint = new ConnectionEndpoint
        {
            Hostname = "127.0.0.1", // From your server configuration.
            Port = 2333 // From your server configuration
        };

        var lavaLinkConfig = new LavalinkConfiguration
        {
            Password = "youshallnotpass", // From your server configuration.
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };
        
        await this._discordClient.ConnectAsync();
        await this._discordClient.GetExtension<LavalinkExtension>().ConnectAsync(lavaLinkConfig);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return this._discordClient.DisconnectAsync();
    }
}