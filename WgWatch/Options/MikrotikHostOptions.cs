using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WgWatch.Options;

public class MikrotikHostOptions
{
    public string? Endpoint { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public List<InterfaceOptions> Interfaces { get; set; } = new List<InterfaceOptions>();
}

