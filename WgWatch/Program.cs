using WgWatch.Mikrotik;
using WgWatch.Options;
using WgWatch.Quota;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHttpClient<RestApi>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        });
        services.ConfigureOptions<MikrotikHostOptionsSetup>();
        services.AddHostedService<QuotaWatcher>();
    })
    .Build();

host.Run();
