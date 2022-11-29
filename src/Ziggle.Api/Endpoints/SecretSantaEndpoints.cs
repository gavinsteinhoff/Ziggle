using AutoMapper;
using Azure.Core.Serialization;
using System.Text.Json;

namespace Ziggle.Api.Endpoints;

public class SecretSantaEndpoints
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public SecretSantaEndpoints(
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _logger = loggerFactory.CreateLogger<SecretSantaEndpoints>();
        _mapper = mapper;
    }

    [Function(nameof(SantaList))]
    public HttpResponseData SantaList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "santa")] HttpRequestData req,
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", SqlQuery = "SELECT * FROM c", PartitionKey = "santa")] List<SecretSantaDto> items
    )
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteAsJsonAsync(items);
        return response;
    }

    [Function(nameof(SantaGet))]
    public async Task<HttpResponseData> SantaGet(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "santa/{id}")] HttpRequestData req,
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", Id = "{id}", PartitionKey = "santa")] SecretSantaDto item
    )
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(item);
        return response;
    }

    [Function(nameof(SantaPost))]
    [CosmosDBOutput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", PartitionKey = "santa")]
    public async Task<SecretSantaDto?> SantaPost([HttpTrigger(AuthorizationLevel.Function, "post", Route = "santa")] HttpRequestData req)
    {
        var newItem = await req.ReadFromJsonAsync<SecretSantaNewDto>();
        if (newItem is null || !newItem.IsValid())
            return null;
        return _mapper.Map<SecretSantaDto>(newItem);
    }

    [Function(nameof(SantaPatch))]
    [CosmosDBOutput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", PartitionKey = "santa")]
    public async Task<SecretSantaDto?> SantaPatch(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "santa/{id}")] HttpRequestData req, string id,
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", SqlQuery = "select * from c where c.id = {id} OFFSET 0 LIMIT 1", PartitionKey = "santa")] List<SecretSantaDto> items
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
