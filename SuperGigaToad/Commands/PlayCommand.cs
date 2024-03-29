﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace SuperGigaToad.Commands;

public class PlayCommand : ApplicationCommandModule
{
    private readonly IServiceProvider _serviceProvider;
    
    public PlayCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public static DiscordColor MainColor = new DiscordColor(0x00ffff);
    public static DiscordEmbedBuilder Base => new DiscordEmbedBuilder().WithColor(MainColor);

    private DiscordEmbedBuilder Error(string error)
    {
        return Base
            .WithTitle("Ein Fehler ist aufgetreten!")
            .AddField("Beschreibung", error);
    }
    private DiscordEmbedBuilder Info(string information)
    {
        return Base
            .WithTitle("Update!")
            .AddField("Beschreibung", information);
    }

    private async Task DeleteAfter(InteractionContext ctx, int millis)
    {
        await Task.Delay(5000);
        await ctx.DeleteResponseAsync();
    }
    private async Task<LavalinkNodeConnection?> GetNode(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        if (!lava.ConnectedNodes.Any())
        {
            await ctx.CreateResponseAsync(Error("Es konnte keine Verbindung zu Lavalink aufgebaut werden!"));
            await DeleteAfter(ctx, 5000);
            return null;
        }
        var node = lava.ConnectedNodes.Values.First();
        return node;
    }
    private async Task<DiscordChannel?> GetVoiceChannelOfAuthor(InteractionContext ctx)
    {
        if (ctx.Member?.VoiceState?.Channel is null)  
        {
            await ctx.CreateResponseAsync(Error("Du befindest dich in keinem Kanal!"));
            await DeleteAfter(ctx, 5000);
            return null;
        }

        var channel = ctx.Member.VoiceState.Channel;
        
        if (channel.Type != ChannelType.Voice)
        {
            await ctx.CreateResponseAsync(Error("Du befindest dich in keinem Voice-Kanal!"));
            await DeleteAfter(ctx, 5000);
            return null;
        }

        return channel;
    }
    private async Task<DiscordGuild?> GetVoiceGuildOfAuthor(InteractionContext ctx)
    {
        if (ctx.Member?.VoiceState?.Guild is null)  
        {
            await ctx.CreateResponseAsync(Error("Du befindest dich auf keinem Server!"));
            await DeleteAfter(ctx, 5000);
            return null;
        }

        return ctx.Member.VoiceState.Guild;
    }

    [SlashCommand("play", "Spielt Musik ab!")]
    public async Task Play(InteractionContext ctx, [Option("Inhalt", "Link oder Suchwort")] string content)
    {
        var channel = await GetVoiceChannelOfAuthor(ctx);
        if (channel is null)
            return;

        var node = await GetNode(ctx);
        if (node is null)
            return;
        
        var conn = await node.ConnectAsync(channel);
        
        await ctx.CreateResponseAsync(Info($"Ich bin dem Channel {channel.Name} beigetreten!"));
        
        var loadResult = await node.Rest.GetTracksAsync(content);                       
        if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Error($"Leider konnte zu '{content}' nichts gefunden werden.")));
            await DeleteAfter(ctx, 5000);
            return;
        }
        
        var track = loadResult.Tracks.First();
        
        await conn.PlayAsync(track);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Info($"Spiele jetzt {track.Title}!")));
        await DeleteAfter(ctx, 10000);
    }

    [SlashCommand("pause", "Pausiert die Musik!")]
    public async Task Pause(InteractionContext ctx)
    {
        var guild = await GetVoiceGuildOfAuthor(ctx);
        if (guild is null)
            return;
        
        var node = await GetNode(ctx);
        if (node is null)
            return;

        var conn = node.GetGuildConnection(guild);

        if (conn is null)
        {
            await ctx.CreateResponseAsync(Error("Der Bot spielt aktuell keine Musik ab!"));
            await DeleteAfter(ctx, 5000);
            return;
        }
        
        await conn.PauseAsync();
        await ctx.CreateResponseAsync(Info("Alles klar! Pausiert!"));
        await DeleteAfter(ctx, 5000);
    }

    [SlashCommand("resume", "Spielt die Musik weiter ab!")]
    public async Task Resume(InteractionContext ctx)
    {
        var guild = await GetVoiceGuildOfAuthor(ctx);
        if (guild is null)
            return;
        
        var node = await GetNode(ctx);
        if (node is null)
            return;

        var conn = node.GetGuildConnection(guild);

        if (conn is null)
        {
            await ctx.CreateResponseAsync(Error("Der Bot spielt aktuell keine Musik ab!"));
            await DeleteAfter(ctx, 5000);
            return;
        }
        
        await conn.ResumeAsync();
        await ctx.CreateResponseAsync(Info("Alles klar! Spiele weiter!"));
        await DeleteAfter(ctx, 5000);
    }

    [SlashCommand("stop", "Stoppt die Musik!")]
    public async Task Stop(InteractionContext ctx)
    {
        var guild = await GetVoiceGuildOfAuthor(ctx);
        if (guild is null)
            return;
        
        var node = await GetNode(ctx);
        if (node is null)
            return;

        var conn = node.GetGuildConnection(guild);

        if (conn is null)
        {
            await ctx.CreateResponseAsync(Error("Der Bot spielt aktuell keine Musik ab!"));
            await DeleteAfter(ctx, 5000);
            return;
        }
        
        await conn.StopAsync();
        await ctx.CreateResponseAsync(Info("Alles klar! Stop!"));
        await DeleteAfter(ctx, 5000);
    }

    [SlashCommand("leave", "Verlässt den Kanal!")]
    public async Task Leave(InteractionContext ctx)
    {
        var guild = await GetVoiceGuildOfAuthor(ctx);
        if (guild is null)
            return;
        
        var node = await GetNode(ctx);
        if (node is null)
            return;

        var conn = node.GetGuildConnection(guild);

        if (conn is null)
        {
            await ctx.CreateResponseAsync(Error("Der Bot spielt aktuell keine Musik ab!"));
            await DeleteAfter(ctx, 5000);
            return;
        }
        
        await conn.DisconnectAsync();
        await ctx.CreateResponseAsync(Info("Alles klar! Tschüss :("));
        await DeleteAfter(ctx, 5000);
    }
}