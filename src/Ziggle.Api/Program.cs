using Discord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ziggle.Api.Middleware;
using Ziggle.Api.Services;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services.AddAutoMapper(typeof(SecretSantaProfile));
        services.AddSingleton<DiscordRestService>();
    })
    .ConfigureFunctionsWorkerDefaults(workerApplication =>
    {
        workerApplication.UseWhen<DiscordMiddleware>((context) =>
        {
            return context.FunctionDefinition.InputBindings.Values.First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";
        });
        workerApplication.UseWhen<GuildAdminMiddleware>((context) =>
        {
            return context.FunctionDefinition.InputBindings.Values.First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";
        });
    })
    .Build();

host.Run();
