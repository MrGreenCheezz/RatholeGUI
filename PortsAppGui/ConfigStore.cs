using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortsAppGui
{
    public class ConfigStore
    {
        public string ServerTomlPath { get; set; }
        public string ClientTomlPath { get; set; }
        public string ServerAdress { get; set; }
        public string ClientAdress { get; set; }
        public string ServerRatholePath {  get; set; }
        public string ClientRatholePath { get;set; }
        public string ServerUsername {  get; set; }
        public string ClientUsername { get; set; }
        public string ServerPassword { get; set; }
        public string ClientPassword { get; set; }
    }
}
