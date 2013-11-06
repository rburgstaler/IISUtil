using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Reflection;
using System.IO;

namespace IISUtil
{
    //IIS 6 did not yet support the new ServerManager component that is available in newer version
    //Instead we need to make use of the WMI (Windows Management Instrumentation) to make modifications
    //to the IIS configuration.
    public class IISWMI : IIS
    {
        public override IISSite CreateNewSite(IISServerCommentIdentifier serverComment, String serverBindings, String filePath)
        {
            return IISWMISite.CreateNewSite(serverComment.Value, serverBindings, filePath);
        }

        public override bool DeleteSite(IISIdentifier siteIdentifier)
        {
            return IISWMISite.DeleteSite(siteIdentifier);
        }
    }

    public class IISWMIHelper
    {
        public static bool TryGetSiteID(IISIdentifier Identifier, ref String SiteId)
        {
            if (!(Identifier is IISServerCommentIdentifier)) throw new Exception(String.Format("IISIdentifier's of type {0} are not yet supported in TryGetSiteID", Identifier.GetType().Name));
            DirectoryEntry iis = GetIIsWebService();
            foreach (DirectoryEntry entry in iis.Children)
            {
                if (entry.SchemaClassName.Equals("iiswebserver", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (entry.Properties["ServerComment"].Value.ToString().Equals(Identifier.Value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        SiteId = entry.Name;
                        return true;
                    }

                }
            }
            return false;
        }

        public static DirectoryEntry GetIIsWebServer(String SiteId)
        {
            return new DirectoryEntry(String.Format("IIS://localhost/w3svc/{0}", SiteId));
        }

        public static DirectoryEntry GetIIsWebVirtualDir(String SiteId)
        {
            return new DirectoryEntry(String.Format("IIS://localhost/w3svc/{0}/root", SiteId));
        }
        public static DirectoryEntry GetIIsWebService()
        {
            return new DirectoryEntry(String.Format("IIS://localhost/w3svc"));
        }
    }

    public class IISWMISite : IISSite
    {
        public static bool DeleteSite(IISIdentifier siteIdentifier)
        {
            String id = "";
            //need to be sure that the site exists or else it can throw an error
            if (IISWMIHelper.TryGetSiteID(siteIdentifier, ref id))
            {
                DirectoryEntry webServer = IISWMIHelper.GetIIsWebServer(id);
                webServer.Invoke("Stop", null);
                webServer.DeleteTree();
                return true;
            }
            return false;
        }

        public override String SiteId { get; set; }
        public static IISWMISite CreateNewSite(String serverComment, String serverBindings, String filePath)
        {
            Directory.CreateDirectory(filePath);
            IISWMISite retVal = new IISWMISite();
            //Do not put any bindings in... we will do that after the site is create
            retVal.SiteId = Convert.ToString(IISWMIHelper.GetIIsWebService().Invoke("CreateNewSite", serverComment, new object[0], filePath));
            retVal.SetBindings(serverBindings);
            return retVal;
        }

        //Return null if the site is not to be found
        public static IISWMISite FindSite(IISIdentifier Identifier)
        {
            String id = "";
            //need to be sure that the site exists or else it can throw an error
            if (IISWMIHelper.TryGetSiteID(Identifier, ref id))
            {
                return new IISWMISite()
                {
                    SiteId = id
                };
            }
            return null;
        }

        //http::80:www.abcdefg.com
        //https::443:www.abcdefg.com
        public override void SetBindings(String siteBindings)
        {
            DirectoryEntry webServer = new DirectoryEntry(String.Format("IIS://localhost/w3svc/{0}", SiteId));

            //We need to parse the bindings string
            webServer.Properties["ServerBindings"].Clear();
            IISBindingParser.Parse(siteBindings,
                delegate(IISBinding iisBinding)
                {
                    if (iisBinding.Protocol.Equals("http", StringComparison.CurrentCultureIgnoreCase))
                    {
                        webServer.Properties["ServerBindings"].Add(iisBinding.WMIBindString);
                    }
                });

            webServer.Properties["SecureBindings"].Clear();
            IISBindingParser.Parse(siteBindings,
                delegate(IISBinding iisBinding)
                {
                    if (iisBinding.Protocol.Equals("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        webServer.Properties["SecureBindings"].Add(iisBinding.WMIBindString);
                    }
                });
            webServer.CommitChanges();
        }

        public override void Start()
        {
            try
            {
                IISWMIHelper.GetIIsWebServer(SiteId).Invoke("Start", null);
            }
            catch (Exception exp)
            {
                throw new Exception(String.Format("Inner error: {0} Outer error: {1}", exp.InnerException.Message, exp.Message));
            }
        }

        private T GetVirtualDirPropertyDef<T>(String propertyName, T DefaultValue)
        {
                PropertyValueCollection pc = IISWMIHelper.GetIIsWebVirtualDir(SiteId).Properties["propertyName"];
                return (pc != null) ? (T)Convert.ChangeType(pc.Value, typeof(T)) : DefaultValue;
        }
        private void SetVirtualDirProperty(String propertyName, object propertyValue)
        {
            DirectoryEntry virDir = IISWMIHelper.GetIIsWebVirtualDir(SiteId);
            virDir.Properties[propertyName].Value = propertyValue;
            virDir.CommitChanges();
        }

        public override String DefaultDoc
        {
            get
            {
                return GetVirtualDirPropertyDef<String>("DefaultDoc", "");
            }
            set
            {
                SetVirtualDirProperty("DefaultDoc", value);
            }
        }

        public override String AppPoolId
        {
            get
            {
                return GetVirtualDirPropertyDef<String>("AppPoolId", "");
            }
            set
            {
                SetVirtualDirProperty("AppPoolId", value);
            }
        }

        public override Int32 AccessFlags
        {
            get
            {
                return GetVirtualDirPropertyDef<Int32>("AccessFlags", 0);
            }
            set
            {
                SetVirtualDirProperty("AccessFlags", value);
            }
        }

        public override Int32 AuthFlags
        {
            get
            {
                return GetVirtualDirPropertyDef<Int32>("AuthFlags", 0);
            }
            set
            {
                SetVirtualDirProperty("AuthFlags", value);
            }
        }

        public override void SetASPDotNetVersion(AspDotNetVersion version)
        {
            DirectoryEntry virDir = IISWMIHelper.GetIIsWebVirtualDir(SiteId);
            ScriptMapper.SetASPNetVersion(virDir, version);
            virDir.CommitChanges();
        }
    }

    public delegate void IISBindingHandler(IISBinding iisBinding); 

    public class IISBindingParser
    {
        public static void Parse(String BindStr, IISBindingHandler callBack)
        {
            String[] bindings = BindStr.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String singleBind in bindings)
            {
                String[] bindParams = singleBind.Split(new String[] { ":" }, StringSplitOptions.None);
                if (bindParams.Length < 4) throw new Exception(String.Format("Invalid binding string specified {0}, must be in http::80:www.abcdefg.com form", singleBind));
                callBack(new IISBinding() { Protocol = bindParams[0], IP = bindParams[1], Port = bindParams[2], Host = bindParams[3] });
            }
        }
    }
    
    public class IISBinding
    {
        String _Protocol = "http";
        public String Protocol {
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
        public String BindString
        {
            get
            {
                return String.Format("{0}:{1}:{2}:{3}", Protocol, IP, Port, Host);
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

    

    public class IISIdentifier
    {
        public String Value { get; set; }
        public IISIdentifier(String ID)
        {
            Value = ID;
        }
    }

    public class IISSiteIdIdentifier : IISIdentifier { public IISSiteIdIdentifier(String ID) : base(ID) { } }
    public class IISBindingIdentifier : IISIdentifier { public IISBindingIdentifier(String ID) : base(ID) { } }
    public class IISServerCommentIdentifier : IISIdentifier { public IISServerCommentIdentifier(String ID) : base(ID) { } }



    //The following is examples of what the metabase looks like
    /* 
            <IIsWebServer	Location ="/LM/W3SVC/1962875481"
                    AuthFlags="0"
                    LogPluginClsid="{FF160663-DE82-11CF-BC0A-00AA006111E0}"
                    SSLCertHash="40da7e6686d698efae7172220f4da438025dfeea"
                    SSLStoreName="MY"
                    SecureBindings=":443:mshsca13.cordonco.com"
                    ServerAutoStart="TRUE"
                    ServerBindings=":80:mshsca13.cordonco.com"
                    ServerComment="MSHSCA13"
                >
            </IIsWebServer>
            <IIsFilters	Location ="/LM/W3SVC/1962875481/filters"
                    AdminACL="NOLONGERVALID02eeb9eb376fc8ffad41b6b1ffca589e7fecc3e2649b5ed4b37a42904899c0b776"
                >
            </IIsFilters>
            <IIsWebVirtualDir	Location ="/LM/W3SVC/1962875481/root"
                    AccessFlags="AccessExecute | AccessRead | AccessScript"
                    AppFriendlyName="Default Application"
                    AppIsolated="2"
                    AppRoot="/LM/W3SVC/1962875481/Root"
                    AuthFlags="AuthAnonymous | AuthNTLM"
                    DefaultDoc="index.aspx"
                    DirBrowseFlags="DirBrowseShowDate | DirBrowseShowTime | DirBrowseShowSize | DirBrowseShowExtension | DirBrowseShowLongDate | EnableDefaultDoc"
                    Path="C:\inetpub\MSHSCA13"
                    ScriptMaps=@".asp,C:\WINDOWS\system32\inetsrv\asp.dll,5,GET,HEAD,POST,TRACE
                        .cer,C:\WINDOWS\system32\inetsrv\asp.dll,5,GET,HEAD,POST,TRACE
                        .cdx,C:\WINDOWS\system32\inetsrv\asp.dll,5,GET,HEAD,POST,TRACE
                        ........ bla bla bla ................
                        .refresh,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG"
                    UNCPassword="NOLONGERVALID2ba612e2a71081820aaa907830586d4986babeddced57560e36e6e5e0a6b2f"
                >
            </IIsWebVirtualDir>
            <IIsWebDirectory	Location ="/LM/W3SVC/1962875481/root/aspnet_client"
                    AccessFlags="AccessRead"
                    DirBrowseFlags="0"
                >
            */

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

    public static class AspDotNetWMIVersionConst
    {
        public const string AspNetV1 = "1.0.3705";
        public const string AspNetV11 = "1.1.4322";
        public const string AspNetV2 = "2.0.50727";
        public const string AspNetV4 = "4.0.30319";

        public static String VersionString(AspDotNetVersion version)
        {
            FieldInfo fi = typeof(AspDotNetWMIVersionConst).GetField(version.ToString());
            return (fi == null) ? "" : Convert.ToString(fi.GetValue(null));
        }
    }

    public static class ScriptMapper
    {
        public static void SetASPNetVersion(DirectoryEntry siteDE, AspDotNetVersion newVersion)
        {
            String targetAspNetVersion = AspDotNetWMIVersionConst.VersionString(newVersion);

            //Need to initialize the script maps for the first time if not setup yet
            if (siteDE.Properties["ScriptMaps"].Count == 0)
            {
                foreach (String sc in ScriptMaps) siteDE.Properties["ScriptMaps"].Add(sc);

            }

            //loop through the script maps
            for (int i = 0; i < siteDE.Properties["ScriptMaps"].Count; i++)
            {
                //replace the versions if they exists
                siteDE.Properties["ScriptMaps"][i] = siteDE.Properties["ScriptMaps"][i].ToString().Replace(AspDotNetWMIVersionConst.AspNetV1, targetAspNetVersion);
                siteDE.Properties["ScriptMaps"][i] = siteDE.Properties["ScriptMaps"][i].ToString().Replace(AspDotNetWMIVersionConst.AspNetV11, targetAspNetVersion);
                siteDE.Properties["ScriptMaps"][i] = siteDE.Properties["ScriptMaps"][i].ToString().Replace(AspDotNetWMIVersionConst.AspNetV2, targetAspNetVersion);
                siteDE.Properties["ScriptMaps"][i] = siteDE.Properties["ScriptMaps"][i].ToString().Replace(AspDotNetWMIVersionConst.AspNetV4, targetAspNetVersion);
            }

            siteDE.CommitChanges();
        }

        private static String[] ScriptMaps = new String[]
        {
            @".asp,C:\WINDOWS\system32\inetsrv\asp.dll,5,GET,HEAD,POST,TRACE",
            @".cer,C:\WINDOWS\system32\inetsrv\asp.dll,5,GET,HEAD,POST,TRACE",
            @".cdx,C:\WINDOWS\system32\inetsrv\asp.dll,5,GET,HEAD,POST,TRACE",
            @".asa,C:\WINDOWS\system32\inetsrv\asp.dll,5,GET,HEAD,POST,TRACE",
            @".idc,C:\WINDOWS\system32\inetsrv\httpodbc.dll,5,GET,POST",
            @".shtm,C:\WINDOWS\system32\inetsrv\ssinc.dll,5,GET,POST",
            @".shtml,C:\WINDOWS\system32\inetsrv\ssinc.dll,5,GET,POST",
            @".stm,C:\WINDOWS\system32\inetsrv\ssinc.dll,5,GET,POST",
            @".asax,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".ascx,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".ashx,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".asmx,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".aspx,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".axd,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".vsdisco,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".rem,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".soap,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".config,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".cs,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".csproj,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".vb,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".vbproj,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".webinfo,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".licx,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".resx,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".resources,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".master,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".skin,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".compiled,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".browser,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".mdb,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".jsl,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".vjsproj,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".sitemap,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".msgx,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".ad,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".dd,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".ldd,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".sd,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".cd,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".adprototype,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".lddprototype,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".sdm,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".sdmDocument,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".ldb,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".svc,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,1,GET,HEAD,POST,DEBUG",
            @".mdf,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".ldf,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".java,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".exclude,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG",
            @".refresh,c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll,5,GET,HEAD,POST,DEBUG"
        };

    }
}
