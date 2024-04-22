using ACMEClientLib;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISUtilLib
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

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static void GetAllSites2(OutputMessage outMsg)
        {
            DNSList lst = new DNSList();
            lst.AppendName("*.geodigital.com");
            Byte[] certHash = StringToByteArray("7AB5E888366D3615778B3A56AB0E1B3AED44909F");
            String CertificateStore = "WebHosting";
            bool UpdateCert = false;
            outMsg(BitConverter.ToString(certHash));



            int changeCount = 0;
            ServerManager mgr = new ServerManager();

            foreach (Site iisSite in mgr.Sites)
            {
                outMsg(iisSite.Name);
                outMsg("   ID: " + iisSite.Id.ToString());
                outMsg("   Path: " + iisSite.Applications[0].VirtualDirectories[0].PhysicalPath);

                foreach (var binding in iisSite.Bindings)
                {
                    if (binding.Protocol.Equals("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        String hashStr = BitConverter.ToString(binding.CertificateHash ?? new Byte[0]).Replace("-", "");

                        outMsg("  https: " + binding.Host  + (binding.CertificateStoreName ?? "") + "\\" + hashStr);
                    }
                    else outMsg("  http: " + binding.Host);

                    bool Matches = HostUtil.AtLeastOneCertMatchesBinding(lst, binding.Host);
                    if (Matches)
                    {
                        outMsg($"Site name: {iisSite.Name} Binding Host: {binding.Host} matches");
                        if (UpdateCert)
                        {
                            Byte[] oldValue = binding.CertificateHash ?? new Byte[0];
                            binding.CertificateHash = certHash;
                            binding.CertificateStoreName = CertificateStore;
                            binding.SetAttributeValue("sslFlags", 1); // Enable SNI support
                            outMsg($"Cert: {BitConverter.ToString(oldValue).Replace("-", "")} has been updated to {BitConverter.ToString(certHash).Replace("-", "")} matches");
                            changeCount++;

                        }
                    }

                }

            }
            if (changeCount > 0)
            {

                mgr.CommitChanges();
                outMsg("Changes committed.");
            }
        }
    }
}
