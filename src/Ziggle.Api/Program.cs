using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services.AddAutoMapper(typeof(SecretSantaProfile));
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
