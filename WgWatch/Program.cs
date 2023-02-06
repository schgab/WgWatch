using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using WgWatch.Mikrotik;
using WgWatch.Options;
using WgWatch.Quotas;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHttpClient<RestApi>((provider, client) =>
        {
            var settings = provider.GetRequiredService<MikrotikHostOptions>();
            client.BaseAddress = new Uri(settings.Endpoint!);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue
            ("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.User}:{settings.Password}")));
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        });
        services.ConfigureOptions<MikrotikHostOptionsSetup>();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MikrotikHostOptions>>().Value);
        services.AddHostedService<QuotaWatcher>();
    })
    .UseSystemd()
    .Build();

host.Run();
