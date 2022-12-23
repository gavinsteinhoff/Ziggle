using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Ziggle.Bot.Services;

namespace Ziggle.Bot.Modules;

[Group("intro", "Intro Music Group")]
public class IntroModule : InteractionModuleHelper<SocketInteractionContext>
{
    private readonly IMusicService _musicService;
    private readonly DiscordSocketClient _client;

    private static readonly string _wrongChannelError = "you need to be in a voice channel";
    private static readonly string _badTrackError = "music is invalid or longer than 5 seconds";

    //TODO: Update this to be the database.
    private readonly Dictionary<ulong, string> _userIntroMap = new();

    public IntroModule(IMusicService musicService, DiscordSocketClient discordSocketClient)
    {
        _musicService = musicService;
        _client = discordSocketClient;
        _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
    }

    [SlashCommand("register", "Registers your intro music.", false, RunMode.Async)]
    public async Task Play([Summary(description: "YouTube URL.")] string search)
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel is null)
        {
            await RespondDefaultErrorAsync(_wrongChannelError);
            return;
        }

        var track = await _musicService.GetTrack(search);
        if (track is null || track.Duration > TimeSpan.FromSeconds(5))
        {
            await RespondDefaultErrorAsync(_badTrackError);
            return;
        }

        await _musicService.Play(Context.Guild.Id, channel.Id, search);
        await RespondDefaultAsync();
        _userIntroMap.Add(Context.User.Id, search);
    }

    private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState beginingState, SocketVoiceState endingState)
    {
        if (beginingState.VoiceChannel is not null)
            return;

        if (_userIntroMap.TryGetValue(user.Id, out var introMusic))
            await _musicService.Play(Context.Guild.Id, endingState.VoiceChannel.Id, introMusic);
    }
}
