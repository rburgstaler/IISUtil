﻿using Microsoft.Web.Administration;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISUtilLib
{
    public delegate void IISBindingHandler(IISBinding iisBinding);
    public class IISBindingParser
    {
        public static List<IISBinding> ParseToList(String BindStr)
        {
            List<IISBinding> retVal = new List<IISBinding>();
            Parse(BindStr,
                delegate (IISBinding iisBinding)
                {
                    retVal.Add(iisBinding);
                });
            return retVal;
        }

        //Will pull out the certificate and has for a certificate store name followed by a backslash followed by a cert hash
        //WebHosting\7AB5E888366D3615778B3A56AB0E1B3AED44909F => CertStore = WebHosting, CertHash = 7AB5E888366D3615778B3A56AB0E1B3AED44909F
        public static void ExtractCertParts(String CertBindingStr, out String CertStore, out String CertHash)
        {
            CertStore = "";
            CertHash = "";
            String[] certParts = CertBindingStr.Split(new String[] { "\\" }, StringSplitOptions.None);
            if (certParts.Length >= 2)
            {
                CertStore = certParts[0];
                CertHash = certParts[1];
            }
            else
            {
                CertHash = certParts[0];
            }
        }

        public static void Parse(String BindStr, IISBindingHandler callBack)
        {
            String[] bindings = BindStr.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String singleBind in bindings)
            {
                String[] bindParams = singleBind.Split(new String[] { ":" }, StringSplitOptions.None);
                if (bindParams.Length < 4) throw new Exception(String.Format("Invalid binding string specified {0}, must be in http::80:www.abcdefg.com:CertStoreName\\a03083aabcd6bdfec92214df7e885c9e1e1a864d form", singleBind));
                IISBinding cb = new IISBinding()
                {
                    Protocol = bindParams[0],
                    IP = bindParams[1],
                    Port = bindParams[2],
                    Host = bindParams[3]
                };
                //If a cert was specified then set that up as well
                if (bindParams.Length >= 5)
                {
                    String certStore, certHash;
                    ExtractCertParts(bindParams[4], out certStore, out certHash);
                    cb.CertificateStore = certStore;
                    cb.CertificateHash = certHash;
                }
                callBack(cb);
            }
        }

    }

    public class IISBindingConverter
    {
        public static IISBinding SMBinding2IISBinding(Binding binding)
        {
            IISBinding retVal = new IISBinding();
            retVal.Protocol = binding.Protocol.ToLower();
            if (binding.Protocol.Equals("https", StringComparison.CurrentCultureIgnoreCase))
            {
                retVal.CertificateHash = SSLCertificates.ByteArrayToHexString(binding.CertificateHash);
                retVal.CertificateStore = binding.CertificateStoreName ?? "";
            }
            //https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.administration.binding.bindinginformation?view=iis-dotnet
            string[] bindParts = binding.BindingInformation.Split(new string[] { ":" }, StringSplitOptions.None);
            if (bindParts.Length > 0) retVal.IP = bindParts[0];
            if (bindParts.Length > 1) retVal.Port = bindParts[1];
            if (bindParts.Length > 2) retVal.Host = bindParts[2];
            return retVal;
            if (bindParts.Length>0) retVal.IP = bindParts[0];
        }
    }
        


    public class IISBinding
    {
        public IISBinding()
        {
            IP = "";
            Port = "";
            Host = "";
            CertificateHash = "";
            CertificateStore = "";
        }

        String _Protocol = "http";
        public String Protocol
        {
            get
            {
                return _Protocol;
            }
            set
            {
                _Protocol = (String.IsNullOrEmpty(value)) ? "http" : value.ToLower();
            }
        }
        public String IP { get; set; }
        public String Port { get; set; }
        public String Host { get; set; }
        public String CertificateHash { get; set; }
        public String CertificateStore { get; set; }
        public String BindString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Protocol).Append(":");
                sb.Append((IP == "") ? "*" : IP).Append(":");
                sb.Append(Port).Append(":");
                sb.Append(Host);
                if (CertificateHash != "") sb.Append(":").Append(CertificateStore).Append("\\").Append(CertificateHash);
                return sb.ToString();
            }
        }

        //Server manager compatible bind string format (does not include the protocol)
        public String SMBindString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append((IP == "") ? "*" : IP).Append(":");
                sb.Append(Port).Append(":");
                sb.Append(Host);
                return sb.ToString();
            }
        }
    }
}
