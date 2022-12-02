using System.Net.Http.Json;
using Ziggle.Models;

namespace Ziggle.Web.Services;

public class UserService
{
    public bool LoggedIn { get; set; } = false;
    public string Username
    {
        get
        {
            return User is null ? "" : User.Username;
        }
    }
    public ZiggleUser? User { get; set; } = null;

    private readonly HttpClient _httpClient;

    public UserService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("http://localhost:7026/api/");
    }

    public async Task Login(string token)
    {
        _httpClient.DefaultRequestHeaders.Add("authorization", "Bearer " + token);
        User = await _httpClient.GetFromJsonAsync<ZiggleUser>("user/self");
        LoggedIn = true;
    }
}
