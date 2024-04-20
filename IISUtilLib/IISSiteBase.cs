using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace IISUtilLib
{
    public class IISIdentifier
    {
        public String Value { get; set; }
        public IISIdentifier(String ID)
        {
            Value = ID;
        }
    }

    public abstract class IIS
    {
        //The following method determines whether WMI or ServerManager based logic
        //is required.
        private static IIS _IIS = null;
        public static IIS Tools
        {
            get
            {
                if (_IIS == null)
                {
                    if (Version.ProductMajorPart <= 8) throw new Exception($"This version of IIS is not support.  Version 8 and less did not support ServerManager and has been deprecated.");
                    _IIS = new IISSM();
                }
                return _IIS;
            }
        }

        public static FileVersionInfo Version
        {
            get
            {
                string w3wpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"inetsrv\w3wp.exe");
                return FileVersionInfo.GetVersionInfo(w3wpPath);
            }
        }

        public abstract IISSite CreateNewSite(IISServerCommentIdentifier serverComment, String serverBindings, String filePath);
        public abstract bool DeleteSite(IISIdentifier siteIdentifier);
        public abstract IISSite FindSite(IISIdentifier siteIdentifier);

    }

    public abstract class IISSite
    {
        //http::80:www.abcdefg.com
        //https::443:www.abcdefg.com
        public abstract void SetBindings(String siteBindings);
        public abstract void Start();
        public abstract String DefaultDoc { get; set; }
        public abstract String AppPoolId { get; set; }
        public abstract Int32 AccessFlags { get; set; }
        public abstract Int32 AuthFlags { get; set; }
        public abstract void SetASPDotNetVersion(AspDotNetVersion version);
        public abstract String SiteId { get; set; }
        public abstract IISBinding FindBinding(IISBinding iisBinding);
    }

    public enum AspDotNetVersion
    {
        //These values MUST match the values in AspDotNetVersionConst.  This approach might be a bad programming practice
        AspNetV1, AspNetV11, AspNetV2, AspNetV4
    }
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

    public class IISSiteIdIdentifier : IISIdentifier { public IISSiteIdIdentifier(String ID) : base(ID) { } }
    public class IISBindingIdentifier : IISIdentifier { public IISBindingIdentifier(String ID) : base(ID) { } }
    public class IISServerCommentIdentifier : IISIdentifier { public IISServerCommentIdentifier(String ID) : base(ID) { } }


    public static class AccessFlags
    {
        //http://msdn.microsoft.com/en-us/library/ms525016(v=vs.90).aspx
        //A value of true indicates that the file or the contents of the folder may be executed, regardless of file type.
        public const int AccessExecute = 0x00000004;
        //A value of true indicates that access to the physical path is not allowed.
        public const int AccessNoPhysicalDir = 0x00008000;
        //A value of true indicates that remote requests to execute applications are denied; only requests from the same 
        //computer as the IIS server succeed if the AccessExecute property is set to true. You cannot set AccessNoRemoteExecute 
        //to false to enable remote requests, and set AccessExecute to false to disable local requests.
        public const int AccessNoRemoteExecute = 0x00002000;
        //A value of true indicates that remote requests to view files are denied; only requests from the same computer as 
        //the IIS server succeed if the AccessRead property is set to true. You cannot set AccessNoRemoteRead to false to 
        //enable remote requests, and set AccessRead to false to disable local requests.
        public const int AccessNoRemoteRead = 0x00001000;
        //A value of true indicates that remote requests to view dynamic content are denied; only requests from the same 
        //computer as the IIS server succeed if the AccessScript property is set to true. You cannot set AccessNoRemoteScript 
        //to false to enable remote requests, and set AccessScript to false to disable local requests.
        public const int AccessNoRemoteScript = 0x00004000;
        //A value of true indicates that remote requests to create or change files are denied; only requests from the 
        //same computer as the IIS server succeed if the AccessWrite property is set to true. You cannot set AccessNoRemoteWrite 
        //to false to enable remote requests, and set AccessWrite to false to disable local requests.
        public const int AccessNoRemoteWrite = 0x00000400;
        //A value of true indicates that the file or the contents of the folder may be read through Microsoft Internet Explorer.
        public const int AccessRead = 0x00000001;
        //A value of true indicates that the file or the contents of the folder may be executed if they are script files or 
        //static content. A value of false only allows static files, such as HTML files, to be served.
        public const int AccessScript = 0x00000200;
        //A value of true indicates that users are allowed to access source code if either Read or Write permissions are set. 
        //Source code includes scripts in Microsoft ® Active Server Pages (ASP) applications.
        public const int AccessSource = 0x00000010;
        //A value of true indicates that users are allowed to upload files and their associated properties to the enabled 
        //directory on your server or to change content in a Write-enabled file. Write can be implemented only with a browser 
        //that supports the PUT feature of the HTTP 1.1 protocol standard.
        public const int AccessWrite = 0x00000002;
    }

    public static class AuthFlags
    {
        //http://msdn.microsoft.com/en-us/library/ms524513(v=vs.90).aspx

        //Specifies Anonymous authentication as one of the possible Windows authentication schemes returned to clients as being available.
        public const int AuthAnonymous = 0x00000001;
        //Specifies Basic authentication as one of the possible Windows authentication schemes returned to clients as being available.
        public const int AuthBasic = 0x00000002;
        //Specifies Digest authentication and Advanced Digest authentication as one of the possible Windows authentication schemes 
        //returned to clients as being available.
        public const int AuthMD5 = 0x00000010;
        //Specifies Integrated Windows authentication (formerly known as Challenge/Response or NTLM authentication) as one of 
        //the possible Windows authentication schemes returned to clients as being available. Windows authentication schemes can 
        //be configured via the NTAuthenticationProviders property.
        public const int AuthNTLM = 0x00000004;
        //A value of true indicates that Microsoft ® .NET Passport authentication is enabled. For more information, see .NET Passport 
        //Authentication in the Help that comes with IIS Manager.
        public const int AuthPassport = 0x00000040;
    }


}
