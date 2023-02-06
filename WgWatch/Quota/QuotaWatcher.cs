using Microsoft.Extensions.Options;
using WgWatch.Mikrotik;
using WgWatch.Options;

namespace WgWatch.Quota;

public class QuotaWatcher : BackgroundService
{
    private const int Interval = 1000 * 5;
    private readonly ILogger<QuotaWatcher> _logger;
    private readonly RestApi _restApi;
    private readonly MikrotikHostOptions _hostOptions;
    private readonly List<Quota> _quotas = new List<Quota>();
    public QuotaWatcher(ILogger<QuotaWatcher> logger, RestApi restApi, IOptions<MikrotikHostOptions> hostOptions)
    {
        _logger = logger;
        _restApi = restApi;
        _hostOptions = hostOptions.Value;
    }

    private async Task Setup()
    {
        var interfaces = await _restApi.ReadInterfaces();
        
        foreach (var interfaceConfig in _hostOptions.Interfaces)
        {
            var monitoredInterface = interfaces?.FirstOrDefault(i => i.Name == interfaceConfig.Name);
            if (monitoredInterface is null)
            {
                _logger.LogWarning($"{interfaceConfig.Name} was not found on Mikrotik. Skipping");
                continue;
            }
            var quota = new Quota(monitoredInterface)
            {
                Action = interfaceConfig.Action,
                Period = interfaceConfig.Period,
                QuotaLimit = interfaceConfig.Quota,
            };
            quota.LoadFromFile();
            quota.SaveToFile();
            _quotas.Add(quota);
        }
        var interfaceNames = _quotas.Select(q => q.MonitoredInterface.Name).Aggregate((a, b) => a + "," + b);
        _logger.LogInformation($"Loaded {_quotas.Count} interfaces to be monitored: {interfaceNames}");
    }

    private async Task UpdateInterfaces()
    {
        var interfaces = await _restApi.ReadInterfaces();
        if (interfaces is null)
        {
            _logger.LogWarning($"Could not read interfaces from Mikrotik or returned empty list");
            return;
        }
        foreach (var quota in _quotas)
        {
            quota.MonitoredInterface = interfaces.First(i => i.Name == quota.MonitoredInterface.Name);
        }
    }

    private async Task EvaluateQuota(Quota quota)
    {
        if (quota.TrafficUsedGigabytes > quota.QuotaLimit && quota.EndDate > DateTime.Now)
        {
            if (quota.Action is ActionOnQuotaExceeded.None)
            {
                return;
            }
            _logger.LogWarning($"Shutting down interface {quota.MonitoredInterface.Name}. Limit of {quota.QuotaLimit}GB is exceeded");
            await _restApi.DisableInterface(quota.MonitoredInterface);
            return;
        }

        if (quota.EndDate < DateTime.Now)
        {
            if (quota.Action is ActionOnQuotaExceeded.Auto)
            {
                await _restApi.EnableInterface(quota.MonitoredInterface);
            }

            await _restApi.ResetTrafficCounter(quota.MonitoredInterface);
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Setup();
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateInterfaces();
            foreach (var quota in _quotas)
            {
                await EvaluateQuota(quota);
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }
}
