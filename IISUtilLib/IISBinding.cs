using System;
using System.Collections.Generic;
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
                    String[] CertStoreCertHash = bindParams[4].Split(new String[] { "\\" }, StringSplitOptions.None);
                    if (CertStoreCertHash.Length >= 2)
                    {
                        cb.CertificateStore = CertStoreCertHash[0];
                        cb.CertificateHash = CertStoreCertHash[1];
                    }
                    else
                    {
                        cb.CertificateHash = CertStoreCertHash[0];
                    }
                }
                callBack(cb);
            }
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
                return String.Format("{0}:{1}:{2}:{3}:{4}\\{5}", Protocol, IP, Port, Host, CertificateStore, CertificateHash);
            }
        }
        //IIS 6 compatible bind string format (does not include the protocol)
        public String WMIBindString
        {
            get
            {
                return String.Format("{0}:{1}:{2}", (IP == "*") ? "" : IP, Port, Host);
            }
        }
        //Server manager compatible bind string format (does not include the protocol)
        public String SMBindString
        {
            get
            {
                return String.Format("{0}:{1}:{2}", (IP == "") ? "*" : IP, Port, Host);
            }
        }
    }
}
