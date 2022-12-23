using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using System.Diagnostics;

namespace Ziggle.Bot.Services;

public interface IMusicService
{
    Task<bool> Play(ulong guildId, ulong channelId, string search);
    Task<bool> Stop(ulong guildId, ulong channelId);
    Task<LavalinkTrack?> CurrentlyPlaying(ulong guildId, ulong channelId);
    Task<LavalinkTrack?> GetTrack(string search);
}

public class MusicService : IMusicService
{
    private readonly IAudioService _audioService;

    public MusicService(IAudioService audioService, DiscordSocketClient client)
    {
        _audioService = audioService;
        client.Ready += () => _audioService.InitializeAsync();

        var info = new ProcessStartInfo("Resources\\run.bat");
        info.UseShellExecute = true;
        Process.Start(info);
    }

    public async Task<bool> Play(ulong guildId, ulong channelId, string search)
    {
        var player = await GetPlayer(guildId, channelId);
        if (player is null)
            return false;

        var track = await GetTrack(search);
        if (track is null)
            return false;

        await player.PlayAsync(track);
        return true;
    }

    public async Task<bool> Stop(ulong guildId, ulong channelId)
    {
        var player = await GetPlayer(guildId, channelId);
        if (player is null)
            return false;

        await player.StopAsync();
        return true;
    }

    public async Task<LavalinkTrack?> CurrentlyPlaying(ulong guildId, ulong channelId)
    {
        var player = await GetPlayer(guildId, channelId);
        if (player is null)
            return null;

        return player.CurrentTrack;
    }

    public async Task<LavalinkTrack?> GetTrack(string search)
    {
        if (string.IsNullOrEmpty(search))
            return null;

        return await _audioService.GetTrackAsync(search, SearchMode.YouTube);
    }

    private async Task<LavalinkPlayer?> GetPlayer(ulong guildId, ulong channelId)
    {
        var player = _audioService.GetPlayer<LavalinkPlayer>(guildId)
            ?? await _audioService.JoinAsync<LavalinkPlayer>(guildId, channelId);

        if (player.VoiceChannelId is null)
            await player.ConnectAsync(channelId);

        return player;
    }
}
