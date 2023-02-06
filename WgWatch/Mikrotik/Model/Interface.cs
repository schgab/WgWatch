using System.Data.Common;
using System.Text.Json.Serialization;

namespace WgWatch.Mikrotik.Model;

public class Interface
{
    [JsonPropertyName(".id")]
    public string? Id { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("disabled")]
    public bool IsDisabled { get; set; }
    [JsonPropertyName("rx-byte")]
    public ulong RxByte { get; set; }
    [JsonPropertyName("tx-byte")]
    public ulong TxByte { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}