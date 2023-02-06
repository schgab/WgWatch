using WgWatch.Mikrotik;
using WgWatch.Options;

namespace WgWatch.Quotas;

public class QuotaWatcher : BackgroundService
{
    private const int Minute = 1000 * 60; //1 minute
    private readonly ILogger<QuotaWatcher> _logger;
    private readonly RestApi _restApi;
    private readonly MikrotikHostOptions _hostOptions;
    private readonly List<Quota> _quotas = new();
    public QuotaWatcher(ILogger<QuotaWatcher> logger, RestApi restApi, MikrotikHostOptions hostOptions)
    {
        _logger = logger;
        _restApi = restApi;
        _hostOptions = hostOptions;
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
            var interf = interfaces.FirstOrDefault(i => i.Name == quota.MonitoredInterface.Name);
            if (interf is null)
            {
                _logger.LogError($"Interface {quota.MonitoredInterface.Name} was not found on Mikrotik! Perhaps it doesn't exist anymore");
                continue;
            }
            quota.MonitoredInterface = interf;
        }
    }

    private async Task EvaluateQuota(Quota quota)
    {
        switch (quota.EvaluateQuotaAction())
        {
            case ActionToPerform.None:
                break;
            case ActionToPerform.DisableInterface:
                _logger.LogWarning($"Shutting down interface {quota.MonitoredInterface.Name}. Limit of {quota.QuotaLimit}GB is exceeded");
                await _restApi.DisableInterface(quota.MonitoredInterface);
                break;
            case ActionToPerform.EnableInterface:
                _logger.LogInformation($"Enabled {quota.MonitoredInterface.Name}");
                await _restApi.EnableInterface(quota.MonitoredInterface);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (quota.IsOutsideMonitoringPeriod)
        {
            _logger.LogInformation($"Resetting traffic counter for interface {quota.MonitoredInterface.Name}");
            await _restApi.ResetTrafficCounter(quota.MonitoredInterface);
            quota.StartDate = DateTime.Now;
            quota.SaveToFile();
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
            await Task.Delay(Minute * _hostOptions.IntervalInMinutes, stoppingToken);
        }
    }
}
