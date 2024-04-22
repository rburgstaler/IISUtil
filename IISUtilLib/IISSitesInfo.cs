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
                    IISBinding iisB = IISBindingConverter.SMBinding2IISBinding(binding);
                    outMsg("Binding: " + iisB.BindString);
                }
            }
        }


        public static void GetAllSites2(OutputMessage outMsg)
        {
            DNSList lst = new DNSList();
            lst.AppendName("*.geodigital.com");
            Byte[] certHash = SSLCertificates.HexStringToByteArray("7AB5E888366D3615778B3A56AB0E1B3AED44909F");
            String CertificateStore = "WebHosting";
            bool UpdateCert = false;
            outMsg(BitConverter.ToString(certHash));



            int changeCount = 0;
            ServerManager mgr = new ServerManager();

            foreach (Site iisSite in mgr.Sites)
            {
                outMsg("Name: " + iisSite.Name);
                outMsg("   ID: " + iisSite.Id.ToString());
                outMsg("   Path: " + iisSite.Applications[0].VirtualDirectories[0].PhysicalPath);

                foreach (var binding in iisSite.Bindings)
                {
                    IISBinding iisB = IISBindingConverter.SMBinding2IISBinding(binding);
                    outMsg("   Binding: " + iisB.BindString);

                    bool Matches = HostUtil.AtLeastOneCertMatchesBinding(lst, binding.Host);
                    if (Matches)
                    {
                        outMsg($"==== Matching cert ===== Site name: {iisSite.Name} Binding Host: {binding.Host} matches {lst.Delimited}");
                        if (UpdateCert)
                        {
                            Byte[] oldValue = binding.CertificateHash ?? new Byte[0];
                            binding.CertificateHash = certHash;
                            binding.CertificateStoreName = CertificateStore;
                            binding.SetAttributeValue("sslFlags", 1); // Enable SNI support
                            outMsg($"Cert: {SSLCertificates.ByteArrayToHexString(oldValue)} has been updated to {SSLCertificates.ByteArrayToHexString(certHash)} matches");
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
