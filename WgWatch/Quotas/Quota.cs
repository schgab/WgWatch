using WgWatch.Mikrotik.Model;
using WgWatch.Options;

namespace WgWatch.Quotas;

public class Quota
{
    public Quota(Interface monitoredInterface)
    {
        MonitoredInterface = monitoredInterface;
    }

    public DateTime StartDate { get; set; } = DateTime.Now;
    public Interface MonitoredInterface { get; set; }
    public double TrafficUsedGigabytes => (MonitoredInterface.RxByte + MonitoredInterface.TxByte) / Math.Pow(1024,3);
    public double QuotaLimit { get; init; }
    public int Period { get; init; }
    public bool IsOutsideMonitoringPeriod => (StartDate.AddDays(Period) < DateTime.Now);
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
    /// <summary>
    /// Evaluates if the interface needs to be shut
    /// </summary>
    /// <returns>The action that need to be performed</returns>
    public ActionToPerform EvaluateQuotaAction()
    {
        //Ignore interface if it is commented with wgwatch-ignore on mikrotik
        if (MonitoredInterface.Comment is not null &&
            MonitoredInterface.Comment.Contains("wgwatch-ignore", StringComparison.OrdinalIgnoreCase))
        {
            return ActionToPerform.None;
        }
        if (TrafficUsedGigabytes > QuotaLimit && !IsOutsideMonitoringPeriod)
        {
            if (Action is ActionOnQuotaExceeded.Auto or ActionOnQuotaExceeded.Shut && !MonitoredInterface.IsDisabled)
            {
                return ActionToPerform.DisableInterface;
            }
        }
        if (!IsOutsideMonitoringPeriod)
        {
            return ActionToPerform.None;
        }
        return Action is ActionOnQuotaExceeded.Auto ? ActionToPerform.EnableInterface : ActionToPerform.None;
    }
    
}