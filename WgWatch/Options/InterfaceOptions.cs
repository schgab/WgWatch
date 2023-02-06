namespace WgWatch.Options;

public class InterfaceOptions
{
    public string? Name { get; set; }
    public double Quota { get; set; }
    public int Period { get; set; }
    public ActionOnQuotaExceeded Action { get; set; }
}

public enum ActionOnQuotaExceeded
{
    None, 
    Shut, //Shuts the interface on quota excess
    Auto  // Same as shut, but enables the interface as soon as Period is up
}