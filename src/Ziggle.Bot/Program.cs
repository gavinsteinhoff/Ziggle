using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ziggle.Bot;
using Ziggle.Bot.Services;

class Program
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    private readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged
    };

    public Program()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        _services = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
            .AddSingleton<IAudioService, LavalinkNode>()
            .AddSingleton(new LavalinkNodeOptions
            {
                RestUri = "http://localhost:2333/",
                WebSocketUri = "ws://localhost:2333/",
                Password = "password",
                AllowResuming = true,
                BufferSize = 1024 * 1024,
                DisconnectOnStop = false,
                ReconnectStrategy = ReconnectStrategies.DefaultStrategy,
                DebugPayloads = false,
            })
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<IMusicService, MusicService>()
            .BuildServiceProvider();
    }

    static void Main()
          => new Program().RunAsync()
              .GetAwaiter()
              .GetResult();

    public async Task RunAsync()
    {
        var client = _services.GetRequiredService<DiscordSocketClient>();
        client.Log += LogAsync;
        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
        await client.LoginAsync(TokenType.Bot, _configuration["BotToken"]);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}