using Discord.Rest;

namespace Ziggle.Models;

public class ZiggleUser
{
    public required string Username { get; set; }
    public required string AvatarUrl { get; set; }
}
