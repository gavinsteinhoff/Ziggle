using Microsoft.Azure.Functions.Worker.Middleware;
using Ziggle.Api.Helpers;
using Ziggle.Api.Services;

namespace Ziggle.Api.Middleware;

public class MiddlewareHelper : IFunctionsWorkerMiddleware
{
    internal readonly DiscordRestService _discordRestService;
    internal HttpRequestData? _httpRequestData;

    public MiddlewareHelper(DiscordRestService discordRestService)
    {
        _discordRestService = discordRestService;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        _httpRequestData = await context.GetHttpRequestDataAsync();
        if (_httpRequestData is null)
            return;

        try
        {
            await HandleLogic(context);
            await next.Invoke(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            var httpResponseData = _httpRequestData.CreateResponse(HttpStatusCode.Unauthorized);
            var invocationResult = context.GetInvocationResult();
            invocationResult.Value = httpResponseData;
        }
        catch
        {
            var httpResponseData = _httpRequestData.CreateResponse(HttpStatusCode.InternalServerError);
            var invocationResult = context.GetInvocationResult();
            invocationResult.Value = httpResponseData;
        }
    }

    internal virtual Task HandleLogic(FunctionContext context)
    {
        return Task.CompletedTask;
    }
}


public class DiscordMiddleware : MiddlewareHelper
{
    public DiscordMiddleware(DiscordRestService discordRestService) : base(discordRestService)
    {
    }

    internal override async Task HandleLogic(FunctionContext context)
    {
        _httpRequestData = await context.GetHttpRequestDataAsync();
        if (_httpRequestData is null)
            return;

        if (!await _discordRestService.LoginClients(_httpRequestData))
            throw new UnauthorizedAccessException();
    }
}

public class GuildAdminMiddleware : MiddlewareHelper
{
    public GuildAdminMiddleware(DiscordRestService discordRestService) : base(discordRestService)
    {
    }

    internal override async Task HandleLogic(FunctionContext context)
    {
        var guildId = RequestHelper.GetGuildId(_httpRequestData!);
        if (guildId is null)
            return;

        var user = _discordRestService.UserRestClient.CurrentUser;
        var guildUser = await _discordRestService.BotRestClient.GetGuildUserAsync(guildId.Value, user.Id);
        if (!guildUser.GuildPermissions.Administrator)
            throw new UnauthorizedAccessException();
    }
}
