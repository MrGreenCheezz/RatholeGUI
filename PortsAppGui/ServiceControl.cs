using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortsAppGui
{
    public partial class ServiceControl : UserControl
    {
        public int Index;
        public delegate void ExitClicked(int index, ServiceControl service);
        public  event ExitClicked? On_ExitButtonClicked;
        public ServiceControl()
        {
            InitializeComponent();
        }

        private void ServiceControl_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        public void SetupControl(Service service)
        {
            ServiceNameTextBox.Text = service.ServiceName;
            ServiceTokenTextBox.Text = service.ServiceToken;
            ClientAdressTextBox.Text = service.ClientAdress;
            ServerAdressTextBox.Text = service.ServerAdress;
            ClientPortTextBox.Text = service.ClientPort;
            ServerPortTextBox.Text = service.ServerPort;
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
            return service;

         }
        public void SetIndex(int index)
        {
            this.Index = index;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            On_ExitButtonClicked?.Invoke(Index, this);
        }
    }
}
