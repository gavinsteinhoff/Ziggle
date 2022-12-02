using AutoMapper;
using Ziggle.Api.Services;

namespace Ziggle.Api.Endpoints;

// TODO: Authentication
public class SecretSantaEndpoints
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly DiscordRestService _discordRestService;

    public SecretSantaEndpoints(
        ILoggerFactory loggerFactory,
        IMapper mapper,
        DiscordRestService discordRestService
    )
    {
        _logger = loggerFactory.CreateLogger<SecretSantaEndpoints>();
        _mapper = mapper;
        _discordRestService = discordRestService;
    }

    [Function(nameof(SantaList))]
    public async Task<HttpResponseData> SantaList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "guild/{guildId}/santa")] HttpRequestData req,
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", SqlQuery = "SELECT * FROM c WHERE c.guildId = {guildId}", PartitionKey = "santa")] List<SecretSantaDto> items
    )
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(items);
        return response;
    }

    [Function(nameof(SantaGet))]
    public async Task<HttpResponseData> SantaGet(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "guild/{guildId}/santa/{id}")] HttpRequestData req,
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", SqlQuery = "SELECT * FROM c WHERE c.Id = {id} AND c.guildId = {guildId} OFFSET 0 LIMIT 1", PartitionKey = "santa")] List<SecretSantaDto> items
    )
    {
        var response = req.CreateResponse(HttpStatusCode.NotFound);
        if (items.Any())
        {
            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items.First());
        }
        return response;
    }

    [Function(nameof(SantaPost))]
    [CosmosDBOutput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", PartitionKey = "santa")]
    public async Task<SecretSantaDto?> SantaPost([HttpTrigger(AuthorizationLevel.Function, "post", Route = "guild/{guildId}/santa")] HttpRequestData req, string guildId)
    {
        await _discordRestService.LoginClients(req);
        var newItem = await req.ReadFromJsonAsync<SecretSantaNewDto>();
        if (newItem is null || !newItem.IsValid())
            return null;

        if (!ulong.TryParse(guildId, out var id))
            return null;

        if (!ulong.TryParse(newItem.GuildRoleId, out var guildRoleId))
            return null;

        var guild = await _discordRestService.BotRestClient.GetGuildAsync(id);
        if (guild.GetRole(guildRoleId) is null)
                return null;

        var saveItem = _mapper.Map<SecretSantaDto>(newItem);
        saveItem.GuildId = guildId;
        return saveItem;
    }

    [Function(nameof(SantaPatch))]
    [CosmosDBOutput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", PartitionKey = "santa")]
    public async Task<SecretSantaDto?> SantaPatch(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "guild/{guildId}/santa/{id}")] HttpRequestData req, string id,
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", SqlQuery = "SELECT * FROM c WHERE c.id = {id} AND c.guildId = {guildId} OFFSET 0 LIMIT 1", PartitionKey = "santa")] List<SecretSantaDto> items
    )
    {
        var newItem = await req.ReadFromJsonAsync<SecretSantaDto>();

        if (newItem is null)
            return null;

        if (newItem.Id != id)
            return null;

        // Find existing item
        var item = items.FirstOrDefault(i => i.Id == id);

        // Error if does not exist
        if (item is null)
            return null;

        // Error is changing Read-only values
        if (newItem.GuildId != item.GuildId)
            return null;

        var saveItem = SetupPatch(newItem, item);

        // TODO: Authorization

        return saveItem;
    }

    /// <summary>
    /// Setups an object to use for patching. Will fill in missing fields.
    /// </summary>
    /// <param name="newItem">The requested item to save.</param>
    /// <param name="existingItem">The existing database item.</param>
    /// <returns>A filled object safe to save.</returns>
    private static SecretSantaDto SetupPatch(SecretSantaDto newItem, SecretSantaDto existingItem)
    {
        var saveItem = new SecretSantaDto
        {
            Id = existingItem.Id,
            GuildId = existingItem.GuildId,
            Name = newItem.Name ?? existingItem.Name,
            GuildRoleId = newItem.GuildRoleId ?? existingItem.GuildRoleId,
            Questions = newItem.Questions ?? existingItem.Questions,
            Members = newItem.Members ?? existingItem.Members
        };

        return saveItem;
    }
}
