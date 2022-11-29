namespace Ziggle.Api.Endpoints;

public class SecretSantaEndpoints
{
    private readonly ILogger _logger;

    public SecretSantaEndpoints(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SecretSantaEndpoints>();
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
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", Id = "{id}", PartitionKey = "santa")] SecretSantaDto item,
        string id
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
        var newItem = await req.ReadFromJsonAsync<SecretSantaDto>();
        if (newItem is null)
            return null;

        var saveItem = CleanNew(newItem);
        return saveItem;
    }

    [Function(nameof(SantaPut))]
    [CosmosDBOutput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", PartitionKey = "santa")]
    public async Task<SecretSantaDto?> SantaPut(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "santa/{id}")] HttpRequestData req, string id,
        [CosmosDBInput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosConnection", SqlQuery = "select * from c where c.id = {id} OFFSET 0 LIMIT 1", PartitionKey = "santa")] List<SecretSantaDto> items
    )
    {
        var newItem = await req.ReadFromJsonAsync<SecretSantaDto>();

        if (newItem is null)
            return null;

        // Find existing item
        var item = items.FirstOrDefault(i => i.Id == id);

        // Create if does not exist
        if (item is null)
            return CleanNew(newItem);

        if (newItem.Id != id)
            return null;

        // Error is changing Read-only values
        if (newItem.GuildId != item.GuildId)
            return null;

        // TODO: Authorization

        return newItem;
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

        var saveItem = CleanPatch(newItem, item);

        // TODO: Authorization

        return saveItem;
    }

    private static SecretSantaDto CleanNew(SecretSantaDto newItem)
    {
        var saveItem = new SecretSantaDto
        {
            Name = newItem.Name,
            GuildId = newItem.GuildId,
            GuildRoleId = newItem.GuildRoleId,
            Questions = newItem.Questions,
            Members = newItem.Members,
        };

        return saveItem;
    }

    private static SecretSantaDto CleanPatch(SecretSantaDto newItem, SecretSantaDto existingItem)
    {
        var saveItem = new SecretSantaDto
        {
            Id = existingItem.Id,
            GuildId = existingItem.GuildId,
            Name = string.IsNullOrEmpty(newItem.Name) ? newItem.Name : newItem.Name,
            GuildRoleId = string.IsNullOrEmpty(newItem.GuildRoleId) ? existingItem.GuildRoleId : newItem.GuildRoleId,
            Questions = newItem.Questions.Any() ? newItem.Questions : existingItem.Questions,
            Members = newItem.Members.Any() ? newItem.Members : existingItem.Members
        };

        return saveItem;
    }
}
