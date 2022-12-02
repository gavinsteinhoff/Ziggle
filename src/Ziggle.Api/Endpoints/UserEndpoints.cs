using Ziggle.Api.Services;

namespace Ziggle.Api.Endpoints;

public class UserEndpoints
{
    private readonly ILogger _logger;
    private readonly DiscordRestService _discordRestService;

    public UserEndpoints(ILoggerFactory loggerFactory, DiscordRestService discordRestService)
    {
        _logger = loggerFactory.CreateLogger<UserEndpoints>();
        _discordRestService = discordRestService;
    }

    [Function(nameof(GetCurrentUser))]
    public async Task<HttpResponseData> GetCurrentUser([HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/self")] HttpRequestData req)
    {
        _logger.LogInformation(nameof(GetCurrentUser) + " called");

        var response = req.CreateResponse();
        var user = new ZiggleUser
        {
            Username = _discordRestService.UserRestClient.CurrentUser.Username,
            AvatarUrl = _discordRestService.UserRestClient.CurrentUser.GetAvatarUrl()
        };
        await response.WriteAsJsonAsync(user);
        return response;
    }
}
