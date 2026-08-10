using PortsAppGui;

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}

static void AssertContains(IEnumerable<string> values, string expectedPart)
{
    Assert(values.Any(value => value.Contains(expectedPart, StringComparison.OrdinalIgnoreCase)),
        $"Expected validation error containing '{expectedPart}'. Actual: {string.Join(" | ", values)}");
}

var config = new ConfigStore
{
    ServerAddress = "server.example.com:22",
    ClientAddress = "client.example.com:22",
    ServerTomlPath = "server.toml",
    ClientTomlPath = "client.toml",
    ServerRatholePath = "/opt/rathole/",
    ClientRatholePath = "/opt/rathole/",
    ServerUsername = "root",
    ClientUsername = "user"
};

var validServices = new[]
{
    new Service
    {
        ServiceName = "web",
        ServiceToken = "secret",
        ClientAddress = "127.0.0.1",
        ClientPort = "8080",
        ServerAddress = "0.0.0.0",
        ServerPort = "8080",
        Enabled = true
    },
    new Service
    {
        ServiceName = "disabled-bad",
        ServiceToken = "",
        ClientAddress = "",
        ClientPort = "not-a-port",
        ServerAddress = "",
        ServerPort = "8080",
        Enabled = false
    }
};

var noErrors = ConfigValidator.Validate(config, validServices);
Assert(noErrors.Count == 0, $"Expected disabled invalid service to be ignored. Actual: {string.Join(" | ", noErrors)}");

var invalidServices = new[]
{
    new Service { ServiceName = "web app", ServiceToken = "", ClientAddress = "", ClientPort = "70000", ServerAddress = "", ServerPort = "8080", Enabled = true },
    new Service { ServiceName = "web app", ServiceToken = "token", ClientAddress = "127.0.0.1", ClientPort = "8080", ServerAddress = "0.0.0.0", ServerPort = "8080", Enabled = true }
};

var errors = ConfigValidator.Validate(config, invalidServices);
AssertContains(errors, "service name");
AssertContains(errors, "token");
AssertContains(errors, "client address");
AssertContains(errors, "client port");
AssertContains(errors, "server port");
AssertContains(errors, "duplicate service name");
AssertContains(errors, "duplicate server port");

var generated = TomlGenerator.GenerateServerConfig(validServices);
Assert(generated.Contains("[server.services.web]"), "Expected enabled service in server TOML.");
Assert(!generated.Contains("disabled-bad"), "Expected disabled service to be skipped in server TOML.");

var matchingState = new ConnectionState
{
    ServerAddress = config.ServerAddress,
    ClientAddress = config.ClientAddress,
    ServerPid = "123",
    ClientPid = "456"
};
Assert(matchingState.Matches(config), "Expected connection state to match current config.");

matchingState.ClientAddress = "other.example.com:22";
Assert(!matchingState.Matches(config), "Expected connection state from another config to be ignored.");

// --- tokens -----------------------------------------------------------------

var token = TokenGenerator.Create();
Assert(token.Length == 24, $"Expected a 24 character token, got {token.Length}.");
Assert(token.All(char.IsLetterOrDigit), $"Token must stay alphanumeric so TOML needs no escaping: '{token}'.");
Assert(TokenGenerator.Create() != TokenGenerator.Create(), "Tokens must not repeat.");

// --- suggestions built from a scanned port ----------------------------------

static ListeningEndpoint Endpoint(string process, string address, int port, string protocol = "TCP") => new()
{
    Protocol = protocol,
    Address = System.Net.IPAddress.Parse(address),
    Port = port,
    Pid = 1234,
    ProcessName = process
};

var anyBound = Endpoint("Minecraft Server", "0.0.0.0", 25565);
Assert(anyBound.BindsAnyAddress, "0.0.0.0 must be treated as a wildcard bind.");
Assert(anyBound.SuggestedLocalAddress == "127.0.0.1",
    $"A wildcard bind should be tunnelled through loopback, got '{anyBound.SuggestedLocalAddress}'.");

var lanBound = Endpoint("qbittorrent", "192.168.1.50", 8080);
Assert(lanBound.SuggestedLocalAddress == "192.168.1.50",
    $"A specific bind address must be kept, got '{lanBound.SuggestedLocalAddress}'.");

var suggestedName = ServiceControl.SuggestServiceName(anyBound);
Assert(suggestedName == "minecraft-server-25565", $"Unexpected suggested name '{suggestedName}'.");
Assert(ConfigValidator.IsValidServiceName(suggestedName),
    $"A suggested name must pass validation, got '{suggestedName}'.");

foreach (var awkward in new[] { "My App (x86)", "steam++.exe", "____", "系统" })
{
    var awkwardName = ServiceControl.SuggestServiceName(Endpoint(awkward, "127.0.0.1", 4000));
    Assert(ConfigValidator.IsValidServiceName(awkwardName),
        $"Name generated from '{awkward}' must be valid for rathole, got '{awkwardName}'.");
}

// --- local port scan --------------------------------------------------------

var scanned = PortScanner.Scan();
Assert(scanned.All(endpoint => endpoint.Port is > 0 and <= 65535),
    "Scanner returned a port outside 1..65535 (byte order bug?).");
Assert(scanned.All(endpoint => endpoint.Protocol is "TCP" or "UDP"), "Scanner returned an unknown protocol.");
Assert(scanned.Select(endpoint => (endpoint.Pid, endpoint.Protocol, endpoint.Port)).Distinct().Count() == scanned.Count,
    "Scanner returned duplicate pid/protocol/port rows; merging is broken.");
Console.WriteLine($"Scanned {scanned.Count} listening endpoint(s) on this machine.");

Console.WriteLine("All tests passed.");
