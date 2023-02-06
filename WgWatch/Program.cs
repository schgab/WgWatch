using System.Net.Http.Headers;
using System.Text;
using WgWatch;
using WgWatch.Mikrotik;
using WgWatch.Options;
using WgWatch.Quota;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
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
