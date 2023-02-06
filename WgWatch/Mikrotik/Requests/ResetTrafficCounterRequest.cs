using System.Text.Json.Serialization;

namespace WgWatch.Mikrotik.Requests;

public class ResetTrafficCounterRequest
{
    [JsonPropertyName(".id")]
    public string? Id { get; init; }
}