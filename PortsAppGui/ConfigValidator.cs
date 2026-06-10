using System.Text.RegularExpressions;

namespace PortsAppGui
{
    public static partial class ConfigValidator
    {
        public static List<string> Validate(ConfigStore config, IEnumerable<Service> services)
        {
            var errors = new List<string>();

            if (!TryParseHostPort(config.ServerAddress, out _, out _))
                errors.Add("Server address must be in host:port format.");
            if (!TryParseHostPort(config.ClientAddress, out _, out _))
                errors.Add("Client address must be in host:port format.");
            if (string.IsNullOrWhiteSpace(config.ServerTomlPath))
                errors.Add("Server TOML path is required.");
            if (string.IsNullOrWhiteSpace(config.ClientTomlPath))
                errors.Add("Client TOML path is required.");
            if (string.IsNullOrWhiteSpace(config.ServerRatholePath))
                errors.Add("Server rathole path is required.");
            if (string.IsNullOrWhiteSpace(config.ClientRatholePath))
                errors.Add("Client rathole path is required.");

            var enabledServices = services.Where(service => service.Enabled).ToList();
            var serviceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var serverPorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var service in enabledServices)
            {
                errors.AddRange(ValidateService(service));

                if (!string.IsNullOrWhiteSpace(service.ServiceName) && !serviceNames.Add(service.ServiceName.Trim()))
                    errors.Add($"Duplicate service name: {service.ServiceName}.");

                if (IsValidPort(service.ServerPort) && !serverPorts.Add(service.ServerPort.Trim()))
                    errors.Add($"Duplicate server port: {service.ServerPort}.");
            }

            return errors;
        }

        public static List<string> ValidateService(Service service)
        {
            var errors = new List<string>();
            if (!service.Enabled)
                return errors;

            if (string.IsNullOrWhiteSpace(service.ServiceName))
                errors.Add("Service name is required.");
            else if (!ServiceNameRegex().IsMatch(service.ServiceName))
                errors.Add($"Service name '{service.ServiceName}' can contain only letters, numbers, underscore and dash.");

            if (string.IsNullOrWhiteSpace(service.ServiceToken))
                errors.Add($"Service '{DisplayName(service)}': token is required.");
            if (string.IsNullOrWhiteSpace(service.ClientAddress))
                errors.Add($"Service '{DisplayName(service)}': client address is required.");
            if (string.IsNullOrWhiteSpace(service.ServerAddress))
                errors.Add($"Service '{DisplayName(service)}': server address is required.");
            if (!IsValidPort(service.ClientPort))
                errors.Add($"Service '{DisplayName(service)}': client port must be 1..65535.");
            if (!IsValidPort(service.ServerPort))
                errors.Add($"Service '{DisplayName(service)}': server port must be 1..65535.");

            return errors;
        }

        public static bool TryParseHostPort(string value, out string host, out int port)
        {
            host = "";
            port = 22;

            var parts = value.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
                return false;

            host = parts[0];
            return int.TryParse(parts[1], out port) && port > 0 && port <= 65535;
        }

        private static bool IsValidPort(string value)
        {
            return int.TryParse(value, out var port) && port > 0 && port <= 65535;
        }

        private static string DisplayName(Service service)
        {
            return string.IsNullOrWhiteSpace(service.ServiceName) ? "<empty>" : service.ServiceName;
        }

        [GeneratedRegex("^[A-Za-z0-9_-]+$")]
        private static partial Regex ServiceNameRegex();
    }
}
