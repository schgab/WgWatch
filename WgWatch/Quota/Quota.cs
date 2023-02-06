using WgWatch.Mikrotik.Model;
using WgWatch.Options;

namespace WgWatch.Quota;

public class Quota
{
    public Quota(Interface monitoredInterface)
    {
        MonitoredInterface = monitoredInterface;
    }

    public DateTime StartDate { get; private set; } = DateTime.Now;
    public DateTime EndDate => StartDate.AddDays(Period);
    public Interface MonitoredInterface { get; set; }
    public double TrafficUsedGigabytes => (MonitoredInterface.RxByte + MonitoredInterface.TxByte) / Math.Pow(1024,3);
    public double QuotaLimit { get; init; }
    public int Period { get; init; }
    public ActionOnQuotaExceeded Action { get; init; }
    public void SaveToFile()
    {
        File.WriteAllText(MonitoredInterface.Name!,StartDate.ToBinary().ToString());
    }

    public void LoadFromFile()
    {
        if (File.Exists(MonitoredInterface.Name))
        {
            StartDate = DateTime.FromBinary(long.Parse(File.ReadAllText(MonitoredInterface.Name)));
        }
    }
}