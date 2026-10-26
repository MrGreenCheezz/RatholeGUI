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

var generated = TomlGenerator.GenerateServerConfig(config, validServices);
Assert(generated.Contains("[server.services.web]"), "Expected enabled service in server TOML.");
Assert(!generated.Contains("disabled-bad"), "Expected disabled service to be skipped in server TOML.");
Assert(generated.Contains("bind_addr = \"0.0.0.0:2333\""), "Expected the default rathole port in server TOML.");
Assert(!generated.Contains("_udp"), "UDP is opt-in: a service without UdpEnabled must not get a UDP twin.");
Assert(!new Service().UdpEnabled, "New services must not forward UDP by default.");

// --- rathole control port ---------------------------------------------------

var customPort = new ConfigStore
{
    ServerAddress = config.ServerAddress,
    ClientAddress = config.ClientAddress,
    ServerTomlPath = config.ServerTomlPath,
    ClientTomlPath = config.ClientTomlPath,
    ServerRatholePath = config.ServerRatholePath,
    ClientRatholePath = config.ClientRatholePath,
    ControlPort = "7000"
};
Assert(TomlGenerator.GenerateServerConfig(customPort, validServices).Contains("bind_addr = \"0.0.0.0:7000\""),
    "Server TOML must bind the configured rathole port.");
Assert(TomlGenerator.GenerateClientConfig(customPort, validServices).Contains("remote_addr = \"server.example.com:7000\""),
    "Client TOML must connect to the configured rathole port.");

var clashing = new[]
{
    new Service { ServiceName = "clash", ServiceToken = "t", ClientAddress = "127.0.0.1", ClientPort = "80", ServerAddress = "0.0.0.0", ServerPort = "7000" }
};
AssertContains(ConfigValidator.Validate(customPort, clashing), "taken by rathole");
customPort.ControlPort = "0";
AssertContains(ConfigValidator.Validate(customPort, validServices), "rathole port");

// --- TOML escaping ----------------------------------------------------------

Assert(TomlGenerator.Quote("plain") == "\"plain\"", "Plain strings must only be wrapped in quotes.");
Assert(TomlGenerator.Quote("a\"b\\c") == "\"a\\\"b\\\\c\"", $"Quotes and backslashes must be escaped, got {TomlGenerator.Quote("a\"b\\c")}.");
Assert(TomlGenerator.Quote("line\nbreak\u0001") == "\"line\\nbreak\\u0001\"", "Control characters must be escaped.");

var hostile = new[]
{
    new Service
    {
        ServiceName = "web", ServiceToken = "tok\"en\ninjected = true", ClientAddress = "127.0.0.1", ClientPort = "8080",
        ServerAddress = "0.0.0.0", ServerPort = "8080", UdpEnabled = true
    }
};
var hostileToml = TomlGenerator.GenerateServerConfig(config, hostile);
Assert(hostileToml.Contains("token = \"tok\\\"en\\ninjected = true\""), $"Token must stay one TOML string:{Environment.NewLine}{hostileToml}");
Assert(!hostileToml.Split('\n').Any(line => line.StartsWith("injected")), "A token must not be able to add TOML keys.");
Assert(hostileToml.Contains("token = \"tok\\\"en\\ninjected = true_udp\""), "The UDP token must be escaped too.");

// --- data.json --------------------------------------------------------------

var tempDir = Directory.CreateTempSubdirectory("ratholegui-tests").FullName;
try
{
    var dataPath = Path.Combine(tempDir, "data.json");
    Assert(DataFile.Load(dataPath, out _) == null, "A missing data.json must load as null.");

    DataFile.Save(dataPath, new JsonDataClass { Configs = customPort, Services = validServices.ToList() });
    Assert(!File.Exists(dataPath + ".tmp"), "The temporary file must be renamed over data.json.");
    var reloaded = DataFile.Load(dataPath, out var noBrokenCopy);
    Assert(noBrokenCopy == null && reloaded != null, "A saved data.json must load back.");
    Assert(reloaded!.Services.Count == 2 && reloaded.Configs.ControlPort == "0", "Saved values must round-trip.");

    File.WriteAllText(dataPath, "{\"Configs\": {\"ServerAdress\": \"half-writ");
    var recovered = DataFile.Load(dataPath, out var brokenCopy);
    Assert(recovered != null && recovered.Services.Count == 0, "A damaged data.json must give an empty configuration.");
    Assert(brokenCopy != null && File.Exists(brokenCopy) && !File.Exists(dataPath),
        "A damaged data.json must be moved aside, not overwritten.");

    File.WriteAllText(dataPath, "{\"Configs\": {\"ServerAdress\": \"old:22\"}}");
    var legacy = DataFile.Load(dataPath, out _)!;
    Assert(legacy.Configs.ControlPort == "2333" && !legacy.Configs.KeepTunnelOnExit,
        "Files written by older versions must get the defaults for new settings.");
}
finally
{
    Directory.Delete(tempDir, recursive: true);
}

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
