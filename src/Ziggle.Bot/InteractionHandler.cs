using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Ziggle.Bot;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;

    public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration config)
    {
        _client = client;
        _handler = handler;
        _services = services;
        _configuration = config;
    }

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;
        _handler.Log += LogAsync;
        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        _client.InteractionCreated += HandleInteraction;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        if (!Program.IsDebug())
        {
            await _handler.RegisterCommandsGloballyAsync(true);
            return;
        }

        var testGuild = _configuration.GetRequiredSection("TestGuild").Value ?? "";
        if (string.IsNullOrEmpty(testGuild))
            return;

        var testGuildId = ulong.Parse(testGuild);
        await _handler.RegisterCommandsToGuildAsync(testGuildId, true);
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _handler.ExecuteCommandAsync(context, _services);
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    default:
                        break;
                }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}
