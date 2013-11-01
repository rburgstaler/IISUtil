using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.DirectoryServices;
using System.IO;
using System.Diagnostics;
using Microsoft.Web.Administration;

namespace IISUtil
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
//            ServerManager sc;
//            Site sit = sc.Sites[""];
//            Microsoft.Web.Administration.Binding bnd = sit.Bindings[0];

            string serverComment = "zzz";
            string path = @"C:\Inetpub\zzz";


            Directory.CreateDirectory(path);
            string serverBindings = ":80:zzz.cordonco.com";
            DirectoryEntry w3svc = new DirectoryEntry("IIS://localhost/w3svc");
            object[] newSite = new object[] { serverComment, new object[] { serverBindings }, path };
            object siteId = (object)w3svc.Invoke("CreateNewSite", newSite);


            DirectoryEntry webServer = new DirectoryEntry(String.Format("IIS://localhost/w3svc/{0}", siteId));
            webServer.Properties["SecureBindings"].Add(":443:zzz.cordonco.com");
            webServer.CommitChanges();
            
            //SetASPNetVersion(w3svc);
            DirectoryEntry virDir = new DirectoryEntry(String.Format("IIS://localhost/w3svc/{0}/root", siteId));
            virDir.Properties["DefaultDoc"].Value = "index.aspx";
            virDir.Properties["AccessFlags"].Value = FlagAccessRead | FlagAccessExecute;

            virDir.Properties["AuthFlags"].Value = AuthFlagsAuthNTLM | AuthFlags;

            virDir.Properties["AppPoolId"].Value = "DotNet4AppPool";
            SetASPNetVersion(virDir);
            virDir.CommitChanges();

            webServer.Invoke("Start", null);
        }

        List<String> siteClass = new List<String>() { "IIsWebServer", "IIsFilters", "IIsWebVirtualDir", "IIsWebDirectory" };


        private bool TryGetSiteID(String Comment, ref String SiteId)
        {
            DirectoryEntry iis = new DirectoryEntry("IIS://localhost/W3SVC");
            foreach (DirectoryEntry entry in iis.Children)
            {
                if (entry.SchemaClassName.Equals("iiswebserver", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (entry.Properties["ServerComment"].Value.ToString().Equals(Comment, StringComparison.CurrentCultureIgnoreCase))
                    {
                        SiteId = entry.Name;
                        return true;
                    }
                    
                }
            }
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string siteId = string.Empty;

            DirectoryEntry iis = new DirectoryEntry("IIS://localhost/W3SVC");

            String id = "";

            TryGetSiteID("zzz", ref id);

            textBox1.Text += id + Environment.NewLine;
            foreach (DirectoryEntry entry in iis.Children)
            {
                if (id.Equals("zzz", StringComparison.CurrentCultureIgnoreCase))
                {
                    textBox1.Text += "DELETING -->" + entry.SchemaClassName + " " + entry.Name + Environment.NewLine;
                }
                else
                {
                    textBox1.Text += entry.SchemaClassName + " " + entry.Name + Environment.NewLine;
                }
            }



            if (id != "")
            {
                DirectoryEntry webServer = new DirectoryEntry("IIS://localhost/W3SVC/" + id);
                webServer.Invoke("Stop", null);

                foreach (DirectoryEntry de in webServer.Children)
                {
                    textBox1.Text += de.Name + Environment.NewLine;
                    
                }
                webServer.DeleteTree();
                //DirectoryEntry virDir = new DirectoryEntry(String.Format("IIS://localhost/W3SVC/{0}/root", id));
                //virDir.Invoke("AppDelete", null);
            }
        
        
        }


        //http://msdn.microsoft.com/en-us/library/ms525016(v=vs.90).aspx
        //AccessFlags
        const int FlagAccessExecute = 0x00000004;
        const int FlagAccessRead = 0x00000001;
        const int AccessFlagsAccessScript = 0x00000200;

        //http://msdn.microsoft.com/en-us/library/ms524513(v=vs.90).aspx
        //AuthFlags
        const int AuthFlagsAuthNTLM = 0x00000004;
        const int AuthFlags = 0x00000001;

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


        private static void SetASPNetVersion(DirectoryEntry siteDE)
        {
            const string AspNetV1 = "1.0.3705";
            const string AspNetV11 = "1.1.4322";
            const string AspNetV2 = "2.0.50727";
            const string AspNetV4 = "4.0.30319";
            const string targetAspNetVersion = AspNetV4; ;

            //Need to initialize the script maps for the first time if not setup yet
            if (siteDE.Properties["ScriptMaps"].Count == 0)
            {
                foreach (String sc in ScriptMaps) siteDE.Properties["ScriptMaps"].Add(sc);
                      
            }

            //loop through the script maps
            for (int i = 0; i < siteDE.Properties["ScriptMaps"].Count; i++)
            {
                //replace the versions if they exists
                siteDE.Properties["ScriptMaps"][i] = siteDE.Properties["ScriptMaps"][i].ToString().Replace(AspNetV1, targetAspNetVersion);
                siteDE.Properties["ScriptMaps"][i] = siteDE.Properties["ScriptMaps"][i].ToString().Replace(AspNetV11, targetAspNetVersion);
                siteDE.Properties["ScriptMaps"][i] = siteDE.Properties["ScriptMaps"][i].ToString().Replace(AspNetV2, targetAspNetVersion);
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

        private void Form1_Load(object sender, EventArgs e)
        {

            string w3wpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"inetsrv\w3wp.exe");
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(w3wpPath);
            Console.WriteLine(versionInfo.ToString());

            if (!CommandLineParameterExists("MyCommand"))  return;
            Console.Error.WriteLine("Standard Error {0}", Environment.GetCommandLineArgs().Length);
            Close();
            // When using a winforms app with AttachConsole the app complets but there is no newline after the process stops. 
            //This gives the newline and looks normal from the console:
            SendKeys.SendWait("{ENTER}");
        }

        public static Boolean CommandLineParameterExists(String AOption)
        {
            String dummy = "";
            return GetCommandLineParameter(AOption, ref dummy);

        }
        public static Boolean GetCommandLineParameter(String AOption, ref String AParamValue)
        {
            AParamValue = "";
            String[] sArgs = Environment.GetCommandLineArgs();
            for (int x = 1; x < sArgs.Length; x++)
            {
                if (String.Compare(AOption, sArgs[x], true) == 0)
                {
                    if ((x + 1) < sArgs.Length) AParamValue = sArgs[x + 1];
                    //The result is true whether there is a corresponding value or not
                    return true;
                }

            }
            return false;
        }

    }
}
