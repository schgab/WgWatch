namespace WgWatch.Options;

public class MikrotikHostOptions
{
    public string? Endpoint { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public List<InterfaceOptions> Interfaces { get; set; } = new List<InterfaceOptions>();
}

