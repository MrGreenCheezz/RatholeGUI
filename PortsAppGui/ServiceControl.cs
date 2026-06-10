using System;
using System.Windows.Forms;

namespace PortsAppGui
{
    public partial class ServiceControl : UserControl
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
            Service service = new Service();
            service.ServiceName = ServiceNameTextBox.Text;
            service.ServiceToken = ServiceTokenTextBox.Text;
            service.ClientAddress = ClientAddressTextBox.Text;
            service.ServerAddress = ServerAddressTextBox.Text;
            service.ClientPort = ClientPortTextBox.Text;
            service.ServerPort = ServerPortTextBox.Text;
            service.NoDelay = NoDelayCheckBox.Checked;
            service.UdpEnabled = UdpCheckBox.Checked;
            service.Enabled = EnabledCheckBox.Checked;
            return service;
        }

        public void SetIndex(int index)
        {
            Index = index;
        }

        public void ApplyValidationState()
        {
            var service = GetServiceData();
            if (!service.Enabled)
            {
                BackColor = Color.Gainsboro;
                return;
            }

            var errors = ConfigValidator.ValidateService(service);
            BackColor = errors.Count == 0 ? Color.Honeydew : Color.MistyRose;
        }

        private void WireValueChangedHandlers()
        {
            foreach (var textBox in Controls.OfType<TextBox>())
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

        private void button1_Click(object sender, EventArgs e)
        {
            ExitClicked?.Invoke(Index, this);
        }
    }
}
