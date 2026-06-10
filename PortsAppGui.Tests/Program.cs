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

Console.WriteLine("All tests passed.");
