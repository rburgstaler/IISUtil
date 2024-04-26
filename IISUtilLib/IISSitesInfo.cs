using ACMEClientLib;
using Microsoft.Web.Administration;
using Org.BouncyCastle.Utilities.Encoders;
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
        //DNSQuery: will filter which sites to return in the result and will filter which hosts to set the new binding cert for
        //UpdateToCertStoreAndHash: When DNSQuerymatches, the cert store and hash will be updated
        public static List<IISSiteInfo> IterateAllSites(OutputMessage outMsg, String DNSQuery = null, String UpdateToCertStoreAndHash = null)
        {
            List<IISSiteInfo> retVal = new List<IISSiteInfo>();
            DNSList lst = new DNSList();
            if (!String.IsNullOrEmpty(DNSQuery)) lst.Delimited = DNSQuery;
            String certStore = "", certHashStr = "";
            Byte[] certHash = new byte[0];
            if (!String.IsNullOrEmpty(UpdateToCertStoreAndHash))
            {
                IISBindingParser.ExtractCertParts(UpdateToCertStoreAndHash, out certStore, out certHashStr);
                certHash = SSLCertificates.HexStringToByteArray(certHashStr);
                String CertificateStore = (certStore == "") ? "WebHosting" : certStore;
            }
            bool UpdateCert = UpdateToCertStoreAndHash != null;

            int webServerChangeCount = 0;
            ServerManager mgr = new ServerManager();

            foreach (Site iisSite in mgr.Sites)
            {
                int webSiteMatchCount = 0;
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
                        webSiteMatchCount++;
                        //outMsg($"==== Matching cert ===== Site name: {iisSite.Name} Binding Host: {binding.Host} matches {lst.Delimited}");
                        if (UpdateCert && (iisB.Protocol.Equals("https", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            Byte[] oldValue = binding.CertificateHash ?? new Byte[0];
                            binding.CertificateHash = certHash;
                            binding.CertificateStoreName = certStore;
                            binding.SetAttributeValue("sslFlags", 1); // Enable SNI support
                            outMsg($"({isf.ID}) - ({isf.Name}) certificate changed: {iisB.BindString} => {IISBindingConverter.SMBinding2IISBinding(binding).BindString}");
                            webServerChangeCount++;

                        }
                    }

                }
                //Only add to the list if we 1.) want to return all [DNSQuery == null], or 2.) have matches
                if ((DNSQuery == null) || (webSiteMatchCount > 0)) retVal.Add(isf);

            }
            if (webServerChangeCount > 0) mgr.CommitChanges();
            if (UpdateCert) outMsg($"{webServerChangeCount} certificate changes committed.");
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
