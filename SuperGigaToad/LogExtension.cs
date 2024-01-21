using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;

namespace SuperGigaToad;

public class LogExtension : BaseExtension
{
    private int _counter;
    private readonly EventId _botEventId = new(42, "SuperGigaToadLog");

    public LogExtension(IServiceProvider serviceProvider)
    {
        this._counter = 0;
    }

    protected override void Setup(DiscordClient discordClient)
    {
        var slashCommandsExtension = discordClient.GetExtension<SlashCommandsExtension>();

        discordClient.GuildAvailable += this.Client_GuildAvailable;
        discordClient.GuildUnavailable += DiscordClientOnGuildUnavailable;
        discordClient.ClientErrored += this.Client_ClientError;
        
        discordClient.MessageCreated += this.DiscordClientOnMessageCreated;
        
        slashCommandsExtension.SlashCommandExecuted += this.SlashCommands_CommandExecuted;
        slashCommandsExtension.SlashCommandErrored += this.SlashCommands_CommandErrored;
    }
    
    private Task DiscordClientOnGuildUnavailable(DiscordClient sender, GuildDeleteEventArgs e)
    {
        int newCounter = Interlocked.Decrement(ref this._counter);
        sender.Logger.LogInformation(this._botEventId, "Guild {Counter} unavailable: {GuildName}", newCounter, e.Guild.Name);
        return Task.CompletedTask;
    }

    private Task DiscordClientOnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        sender.Logger.LogDebug(this._botEventId, "'{0}' wrote in '{2}': '{3}': '{4}'", e.Author?.Username, e.Guild?.Name, e.Channel?.Name, e.Message?.Content);
        return Task.CompletedTask;
    }

    private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        int newCounter = Interlocked.Increment(ref this._counter);
        sender.Logger.LogInformation(this._botEventId, "Guild {Counter} available: {GuildName}", newCounter, e.Guild.Name);
        return Task.CompletedTask;
    }

    private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        // let's log the details of the error that just 
        // occured in our client
        sender.Logger.LogError(this._botEventId, e.Exception, "Exception occured");

        // since this method is not async, let's return
        // a completed task, so that no additional work
        // is done
        return Task.CompletedTask;
    }
    
    private Task SlashCommands_CommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
    {
        // let's log the name of the command and user
        e.Context.Client.Logger.LogInformation(this._botEventId, "{0} successfully executed '{1}'", e.Context.User.Username, e.Context.CommandName);

        // since this method is not async, let's return
        // a completed task, so that no additional work
        // is done
        return Task.CompletedTask;
    }
    private async Task SlashCommands_CommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        // let's log the error details
        e.Context.Client.Logger.LogError(this._botEventId, "{0} tried executing '{1}' but it errored: {2}: {3}", e.Context.User.Username, e.Context.CommandName, e.Exception.GetType(), e.Exception.Message);
    }

    public override void Dispose()
    {
    }
}