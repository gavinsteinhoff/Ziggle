namespace Ziggle.Models;

public class SecretSantaDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [JsonPropertyName("kind")]
    public string Kind { get; } = "santa";

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("guildId")]
    public string GuildId { get; init; } = string.Empty;

    [JsonPropertyName("guildRoleId")]
    public string GuildRoleId { get; init; } = string.Empty;

    [JsonPropertyName("questions")]
    public List<string> Questions { get; init; } = new();

    [JsonPropertyName("members")]
    public List<SecretSantaMemberDto> Members { get; init; } = new();
}

public class SecretSantaMemberDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("answers")]
    public List<string> Answers { get; init; } = new();
}