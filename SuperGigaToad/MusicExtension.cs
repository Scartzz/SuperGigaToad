using DSharpPlus;
using DSharpPlus.SlashCommands;
using SuperGigaToad.Commands;

namespace SuperGigaToad;

public class MusicExtension : BaseExtension
{
    private readonly IServiceProvider _serviceProvider;
    
    public MusicExtension(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void Setup(DiscordClient discordClient)
    {
        var slashCommands = discordClient.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = _serviceProvider,
        });
        
        slashCommands.RegisterCommands<PlayCommand>();
    }
    
    public override void Dispose()
    {
    }
}