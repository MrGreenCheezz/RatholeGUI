using System.Text;
using PortsAppGui.UI;

namespace PortsAppGui
{
    public partial class ServiceControl : CardPanel
    {
        public int Index;
        public delegate void ExitClickedHandler(int index, ServiceControl service);
        public event ExitClickedHandler? ExitClicked;
        public event EventHandler? ValueChanged;

        public ServiceControl()
        {
            InitializeComponent();
            WireValueChangedHandlers();
        }

        public void SetupControl(Service service)
        {
            ServiceNameTextBox.Text = service.ServiceName;
            ServiceTokenTextBox.Text = service.ServiceToken;
            ClientAddressTextBox.Text = service.ClientAddress;
            ServerAddressTextBox.Text = service.ServerAddress;
            ClientPortTextBox.Text = service.ClientPort;
            ServerPortTextBox.Text = service.ServerPort;
            NoDelayCheckBox.Checked = service.NoDelay;
            UdpCheckBox.Checked = service.UdpEnabled;
            EnabledCheckBox.Checked = service.Enabled;
            ApplyValidationState();
        }

        public Service GetServiceData()
        {
            return new Service
            {
                ServiceName = ServiceNameTextBox.Text,
                ServiceToken = ServiceTokenTextBox.Text,
                ClientAddress = ClientAddressTextBox.Text,
                ServerAddress = ServerAddressTextBox.Text,
                ClientPort = ClientPortTextBox.Text,
                ServerPort = ServerPortTextBox.Text,
                NoDelay = NoDelayCheckBox.Checked,
                UdpEnabled = UdpCheckBox.Checked,
                Enabled = EnabledCheckBox.Checked
            };
        }

        public void SetIndex(int index)
        {
            Index = index;
        }

        /// <summary>Repaints the card so its stripe and field frames show the current validation result.</summary>
        public void ApplyValidationState()
        {
            var service = GetServiceData();
            var errors = ConfigValidator.ValidateService(service);

            ServiceNameTextBox.HasError = service.Enabled && !ConfigValidator.IsValidServiceName(service.ServiceName);
            ServiceTokenTextBox.HasError = service.Enabled && string.IsNullOrWhiteSpace(service.ServiceToken);
            ClientAddressTextBox.HasError = service.Enabled && string.IsNullOrWhiteSpace(service.ClientAddress);
            ServerAddressTextBox.HasError = service.Enabled && string.IsNullOrWhiteSpace(service.ServerAddress);
            ClientPortTextBox.HasError = service.Enabled && !ConfigValidator.IsValidPort(service.ClientPort);
            ServerPortTextBox.HasError = service.Enabled && !ConfigValidator.IsValidPort(service.ServerPort);

            if (!service.Enabled)
            {
                StripeColor = Theme.TextDisabled;
                FillColor = Theme.Mix(Theme.Card, Theme.Window, 0.45);
                BorderColor = Theme.Border;
            }
            else if (errors.Count > 0)
            {
                StripeColor = Theme.Danger;
                FillColor = Theme.Card;
                BorderColor = Theme.Mix(Theme.Border, Theme.Danger, 0.5);
            }
            else
            {
                StripeColor = Theme.Success;
                FillColor = Theme.Card;
                BorderColor = Theme.Border;
            }

            BackColor = FillColor;
            Invalidate();
        }

        /// <summary>Fills the client side of this service from a port discovered on this machine.</summary>
        public void ApplyEndpoint(ListeningEndpoint endpoint, bool overwriteName = false)
        {
            ClientAddressTextBox.Text = endpoint.SuggestedLocalAddress;
            ClientPortTextBox.Text = endpoint.Port.ToString();

            if (string.IsNullOrWhiteSpace(ServerPortTextBox.Text))
                ServerPortTextBox.Text = endpoint.Port.ToString();

            if (string.IsNullOrWhiteSpace(ServerAddressTextBox.Text))
                ServerAddressTextBox.Text = "0.0.0.0";

            if (string.IsNullOrWhiteSpace(ServiceTokenTextBox.Text))
                ServiceTokenTextBox.Text = TokenGenerator.Create();

            if (overwriteName || string.IsNullOrWhiteSpace(ServiceNameTextBox.Text))
                ServiceNameTextBox.Text = SuggestServiceName(endpoint);

            if (endpoint.Protocol == "UDP")
                UdpCheckBox.Checked = true;

            ApplyValidationState();
        }

        /// <summary>
        /// Turns a process name into something rathole accepts: ASCII letters, digits, underscore
        /// and dash only. Anything else (spaces, punctuation, non-Latin scripts) collapses to a
        /// single dash, because char.IsLetterOrDigit would happily pass CJK through.
        /// </summary>
        public static string SuggestServiceName(ListeningEndpoint endpoint)
        {
            var builder = new StringBuilder();
            foreach (var character in endpoint.ProcessName.ToLowerInvariant())
            {
                if (character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-')
                    builder.Append(character);
                else if (builder.Length > 0 && builder[^1] != '-')
                    builder.Append('-');
            }

            var name = builder.ToString().Trim('-');
            if (name.Length == 0)
                name = "app";

            return $"{name}-{endpoint.Port}";
        }

        private void WireValueChangedHandlers()
        {
            foreach (var textBox in Controls.OfType<ModernTextBox>())
                textBox.TextChanged += ControlValueChanged;

            NoDelayCheckBox.CheckedChanged += ControlValueChanged;
            UdpCheckBox.CheckedChanged += ControlValueChanged;
            EnabledCheckBox.CheckedChanged += ControlValueChanged;
        }

        private void ControlValueChanged(object? sender, EventArgs e)
        {
            ApplyValidationState();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            ExitClicked?.Invoke(Index, this);
        }

        private void RegenerateTokenButton_Click(object? sender, EventArgs e)
        {
            ServiceTokenTextBox.Text = TokenGenerator.Create();
        }

        private void ScanButton_Click(object? sender, EventArgs e)
        {
            using var picker = new PortPickerForm();
            if (picker.ShowDialog(FindForm()) == DialogResult.OK && picker.SelectedEndpoint != null)
                ApplyEndpoint(picker.SelectedEndpoint);
        }
    }
}
