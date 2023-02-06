using System.Text.Json.Serialization;

namespace WgWatch.Mikrotik.Requests;

public class SetInterfaceAdminStatusRequest
{
    [JsonPropertyName("disabled")]
    public string? Status { get; init; }
}