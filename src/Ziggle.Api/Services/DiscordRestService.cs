using Discord;
using Discord.Rest;
using Ziggle.Api.Helpers;

namespace Ziggle.Api.Services;

public class DiscordRestService
{
    public readonly DiscordRestClient BotRestClient;
    public readonly DiscordRestClient UserRestClient;

    public DiscordRestService()
    {
        BotRestClient = new();
        UserRestClient = new();
    }

    public async Task<bool> LoginClients(HttpRequestData httpRequestData)
    {
        try
        {
            var token = RequestHelper.GetToken(httpRequestData);
            if (token is null)
                return false;

            var botToken = Environment.GetEnvironmentVariable("BotToken");
            await BotRestClient.LoginAsync(TokenType.Bot, botToken);
            await UserRestClient.LoginAsync(TokenType.Bearer, token);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
