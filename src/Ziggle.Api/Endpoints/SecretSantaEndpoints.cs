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

        var saveItem = SecretSantaDto.GetNew(newItem);
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
            return SecretSantaDto.GetNew(newItem);

        if (newItem.Id != id)
            return null;

        // Read-only values
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

        // Read-only values
        if (newItem.GuildId != item.GuildId)
            return null;

        // Patch

        var saveItem = new SecretSantaDto
        {
            Id = item.Id,
            Name = string.IsNullOrEmpty(newItem.Name) ? item.Name : newItem.Name,
            GuildRoleId = string.IsNullOrEmpty(newItem.GuildRoleId) ? item.GuildRoleId : newItem.GuildRoleId,
            Questions = newItem.Questions.Any() ? newItem.Questions : item.Questions,
            Members = newItem.Members.Any() ? newItem.Members : item.Members
        };

        // TODO: Authorization

        return saveItem;
    }
}
