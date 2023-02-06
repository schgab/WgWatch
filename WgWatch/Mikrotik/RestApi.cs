using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WgWatch.Mikrotik.Json;
using WgWatch.Mikrotik.Model;
using WgWatch.Mikrotik.Requests;
using WgWatch.Options;

namespace WgWatch.Mikrotik;
using System.Net.Http.Json;
public class RestApi
{
    private readonly HttpClient _httpClient;
    public RestApi(HttpClient httpClient, IOptions<MikrotikHostOptions> mikrotikHostOptions)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(mikrotikHostOptions.Value.Endpoint!);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue
        ("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{mikrotikHostOptions.Value.User}:{mikrotikHostOptions.Value.Password}")));
    }

    public async Task<List<Interface>?> ReadInterfaces()
    {
        var options = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new BoolConverter() }
        };
        return await _httpClient.GetFromJsonAsync<List<Interface>>("interface",options);
    }
    public async Task ResetTrafficCounter(Interface interf)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "interface/reset-counters");
        requestMessage.Content =
            new StringContent(
                JsonSerializer.Serialize(new ResetTrafficCounterRequest{Id = interf.Id}), Encoding.UTF8, "application/json");
        await _httpClient.SendAsync(requestMessage);
    }

    public async Task DisableInterface(Interface interf)
    {
        await SetInterfaceAdminStatus(interf, false);
    }
    public async Task EnableInterface(Interface interf)
    {
        await SetInterfaceAdminStatus(interf, true);
    }
    public async Task SetInterfaceAdminStatus(Interface interf, bool adminStatus)
    {
        //PatchAsJsonAsync doesn't work
        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"interface/{interf.Name}");
        requestMessage.Content =
            new StringContent(
                JsonSerializer.Serialize(new SetInterfaceAdminStatusRequest
                    { Status = (!adminStatus).ToString().ToLower() }), Encoding.UTF8, "application/json");
        await _httpClient.SendAsync(requestMessage);
    }
}