namespace Ziggle.Api.Helpers;

public static class RequestHelper
{
    public static string? GetToken(HttpRequestData httpRequestData)
    {
        if (!httpRequestData.Headers.TryGetValues("authorization", out var values))
            return default;

        if (!values.First().StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
            return default;

        return values.First()["Bearer ".Length..].Trim();
    }

    public static ulong? GetGuildId(HttpRequestData httpRequestData)
    {
        try
        {
            var segments = httpRequestData.Url.Segments;
            var index = segments.ToList().IndexOf("guild/");
            if (index == -1)
                return null;

            // check if guild/ is not last
            if (segments.Length < index)
                return null;

            // grab the id right after guild/
            var item = segments[index + 1].Replace("/", "");
            return Convert.ToUInt64(item);
        }
        catch
        {
            return null;
        }
    }
}
