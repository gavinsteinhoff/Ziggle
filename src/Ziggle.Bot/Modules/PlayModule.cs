using Discord;
using Discord.Interactions;
using Ziggle.Bot.Services;

namespace Ziggle.Bot.Modules;

[Group("music", "Music Group")]
public class PlayModule : InteractionModuleHelper<SocketInteractionContext>
{
    private readonly IMusicService _musicService;

    private static readonly string _wrongChannelError = "you need to be in a voice channel";
    private static readonly string _nothingPlayingError = "there is nothing playing";

    public PlayModule(IMusicService musicService)
    {
        _musicService = musicService;
    }

    [SlashCommand("play", "Plays music.", false, RunMode.Async)]
    public async Task Play([Summary(description: "Search String")] string search)
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel is null)
        {
            await RespondDefaultErrorAsync(_wrongChannelError);
            return;
        }

        await _musicService.Play(Context.Guild.Id, channel.Id, search);
        await RespondDefaultAsync();
    }

    [SlashCommand("stop", "Stops music.", false, RunMode.Async)]
    public async Task Stop()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel is null)
        {
            await RespondDefaultErrorAsync(_wrongChannelError);
            return;
        }

        await _musicService.Stop(Context.Guild.Id, channel.Id);
        await RespondDefaultAsync();
    }

    [SlashCommand("playing", "Get current music.", false, RunMode.Async)]
    public async Task Playing()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel is null)
        {
            await RespondDefaultErrorAsync(_wrongChannelError);
            return;
        }

        var track = await _musicService.CurrentlyPlaying(Context.Guild.Id, channel.Id);
        if (track is null)
        {
            await RespondDefaultErrorAsync(_nothingPlayingError);
            return;
        }

        await RespondAsync($"{track.Title} <-|-> ${track.Author}", ephemeral: true);
    }
}
