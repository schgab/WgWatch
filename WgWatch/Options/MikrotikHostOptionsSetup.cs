using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace WgWatch.Options;

public class MikrotikHostOptionsSetup : IConfigureOptions<MikrotikHostOptions>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MikrotikHostOptionsSetup> _logger;
    public MikrotikHostOptionsSetup(IConfiguration configuration, ILogger<MikrotikHostOptionsSetup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Configure(MikrotikHostOptions options)
    {
        var filePath = _configuration.GetValue<string>("ConfigFile");
        if(!File.Exists(filePath))
        {
            _logger.LogCritical($"Invalid config filepath: {filePath}! Configure it correctly in appsettings.json. Exiting...");
            Environment.Exit(1);
        }
        var contents = File.ReadAllText(filePath);
        var mikOptions = JsonSerializer.Deserialize<MikrotikHostOptions>(contents, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });
        options.Endpoint = mikOptions!.Endpoint;
        options.Password = mikOptions.Password;
        options.User = mikOptions.User;
        options.Interfaces = mikOptions.Interfaces.ToList();
    }
}