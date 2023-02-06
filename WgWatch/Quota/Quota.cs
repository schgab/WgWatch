using WgWatch.Mikrotik.Model;
using WgWatch.Options;

namespace WgWatch.Quota;

public class Quota
{
    public DateTime StartDate { get; private set; } = DateTime.Now;
    public DateTime EndDate => StartDate.AddDays(Period);
    public Interface MonitoredInterface { get; set; }
    public ulong TrafficUsedGigabytes => (MonitoredInterface?.RxByte ?? 0 + MonitoredInterface?.TxByte ?? 0) / (1024 ^ 3);
    public ulong QuotaLimit { get; set; }
    public int Period { get; set; }
    public ActionOnQuotaExceeded Action { get; set; }
    public void SaveToFile()
    {
        File.WriteAllText(MonitoredInterface.Name,StartDate.ToBinary().ToString());
    }

    public void LoadFromFile()
    {
        if (File.Exists(MonitoredInterface.Name))
        {
            StartDate = DateTime.FromBinary(long.Parse(File.ReadAllText(MonitoredInterface.Name)));
        }
    }
}