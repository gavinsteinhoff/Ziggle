using AutoMapper;

namespace Ziggle.Models;

public class SecretSantaDto : SecretSantaNewDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [JsonPropertyName("kind")]
    public string Kind { get => "santa"; }

    [JsonPropertyName("guildId")]
    public string? GuildId { get; set; }
}

public class SecretSantaNewDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("guildRoleId")]
    public string? GuildRoleId { get; init; }

    [JsonPropertyName("questions")]
    public List<string>? Questions { get; init; }

    [JsonPropertyName("members")]
    public List<SecretSantaMemberDto>? Members { get; init; }

    public bool IsValid()
    {
        return
            !string.IsNullOrEmpty(Name) &&
            !string.IsNullOrEmpty(GuildRoleId);
    }
}

public class SecretSantaMemberDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("answers")]
    public List<string> Answers { get; init; } = new();
}

public class SecretSantaProfile : Profile
{
    public SecretSantaProfile()
    {
        CreateMap<SecretSantaNewDto, SecretSantaDto>();
    }
}