using System;
using System.Windows.Forms;

namespace PortsAppGui
{
    public partial class ServiceControl : UserControl
    {
        public int Index;
        public delegate void ExitClickedHandler(int index, ServiceControl service);
        public event ExitClickedHandler ExitClicked;

        public ServiceControl()
        {
            InitializeComponent();
        }

        public void SetupControl(Service service)
        {
            ServiceNameTextBox.Text = service.ServiceName;
            ServiceTokenTextBox.Text = service.ServiceToken;
            ClientAdressTextBox.Text = service.ClientAdress;
            ServerAdressTextBox.Text = service.ServerAdress;
            ClientPortTextBox.Text = service.ClientPort;
            ServerPortTextBox.Text = service.ServerPort;
            NoDelayCheckBox.Checked = service.NoDelay;
            UdpCheckBox.Checked = service.UdpEnabled;
        }

        public Service GetServiceData()
        {
            Service service = new Service();
            service.ServiceName = ServiceNameTextBox.Text;
            service.ServiceToken = ServiceTokenTextBox.Text;
            service.ClientAdress = ClientAdressTextBox.Text;
            service.ServerAdress = ServerAdressTextBox.Text;
            service.ClientPort = ClientPortTextBox.Text;
            service.ServerPort = ServerPortTextBox.Text;
            service.NoDelay = NoDelayCheckBox.Checked;
            service.UdpEnabled = UdpCheckBox.Checked;
            return service;
        }

        public void SetIndex(int index)
        {
            this.Index = index;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExitClicked?.Invoke(Index, this);
        }
    }
}
