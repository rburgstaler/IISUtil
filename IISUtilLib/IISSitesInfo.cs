using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISConfigLib
{
    public delegate void OutputMessage(String msg);
    public  class IISSitesInfo
    {

        public static void GetAllSites(OutputMessage outMsg)
        {

            ServerManager mgr = new ServerManager();

            foreach (Site iisSite in mgr.Sites)
            {
                outMsg("Name: " + iisSite.Name);
                outMsg("ID: " + iisSite.Id.ToString());
                outMsg("Path: " + iisSite.Applications[0].VirtualDirectories[0].PhysicalPath);

                foreach (var binding in iisSite.Bindings)
                {
                    if (binding.Protocol.Equals("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        String hashStr = BitConverter.ToString(binding.CertificateHash ?? new Byte[0]).Replace("-", "");
                        outMsg($"Bindings: {binding.Protocol}:{binding.BindingInformation}:{binding.CertificateStoreName}\\{hashStr}");
                    }
                    else outMsg($"Bindings: {binding.Protocol}:{binding.BindingInformation}");
                }
            }
        }
    }
}
