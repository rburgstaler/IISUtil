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

        public static List<IISSiteInfo> IterateAllSites(OutputMessage outMsg)
        {
            List<IISSiteInfo> retVal = new List<IISSiteInfo>();
            DNSList lst = new DNSList();
            lst.AppendName("*.contoso.com");
            Byte[] certHash = SSLCertificates.HexStringToByteArray("7AB5E888366D3615778B3A56AB0E1B3AED44909F");
            String CertificateStore = "WebHosting";
            bool UpdateCert = false;
            //outMsg(BitConverter.ToString(certHash));



            int changeCount = 0;
            ServerManager mgr = new ServerManager();

            foreach (Site iisSite in mgr.Sites)
            {
                IISSiteInfo isf = new IISSiteInfo()
                {
                    ID = iisSite.Id,
                    Name = iisSite.Name,
                    Path = iisSite.Applications[0].VirtualDirectories[0].PhysicalPath

                };

                //outMsg("Name: " + iisSite.Name);
                //outMsg("   ID: " + iisSite.Id.ToString());
                //outMsg("   Path: " + iisSite.Applications[0].VirtualDirectories[0].PhysicalPath);

                foreach (var binding in iisSite.Bindings)
                {
                    IISBinding iisB = IISBindingConverter.SMBinding2IISBinding(binding);
                    //outMsg("   Binding: " + iisB.BindString);
                    isf.Bindings.Add(iisB.BindString);

                    bool Matches = HostUtil.AtLeastOneCertMatchesBinding(lst, binding.Host);
                    if (Matches)
                    {
                        //outMsg($"==== Matching cert ===== Site name: {iisSite.Name} Binding Host: {binding.Host} matches {lst.Delimited}");
                        if (UpdateCert)
                        {
                            Byte[] oldValue = binding.CertificateHash ?? new Byte[0];
                            binding.CertificateHash = certHash;
                            binding.CertificateStoreName = CertificateStore;
                            binding.SetAttributeValue("sslFlags", 1); // Enable SNI support
                            //outMsg($"Cert: {SSLCertificates.ByteArrayToHexString(oldValue)} has been updated to {SSLCertificates.ByteArrayToHexString(certHash)} matches");
                            changeCount++;

                        }
                    }

                }
                retVal.Add(isf);

            }
            if (changeCount > 0)
            {

                mgr.CommitChanges();
                //outMsg("Changes committed.");
            }
            return retVal;
        }
    }

    public class IISSiteInfo
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public List<String> Bindings { get; set; } = new List<String>();
    }
}
