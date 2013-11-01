using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.DirectoryServices;
using System.IO;
using System.Diagnostics;
using Microsoft.Web.Administration;

namespace IISUtil
{
    public partial class IISUtilForm : Form
    {
        public IISUtilForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            String si = "not found";
            IISServerCommentIdentifier id = new IISServerCommentIdentifier("zzz");
            IISWMIHelper.TryGetSiteID(id, ref si);
            textBox1.Text += si + Environment.NewLine;
            return;
            */
              
            string serverComment = "zzz";
            string path = @"C:\Inetpub\zzz";
            string serverBindings = "https:*:80:zzz.cordonco.com;https::443:zzz.cordonco.com";
            string appPool = (false) ? "DotNet4AppPool" : ".NET v4.5";


            Directory.CreateDirectory(path);

            //DirectoryEntry w3svc = new DirectoryEntry("IIS://localhost/w3svc");
            //object[] newSite = new object[] { serverComment, new object[] { serverBindings }, path };
            //object siteId = (object)w3svc.Invoke("CreateNewSite", newSite);
            IISWMISite site = IISWMISite.CreateNewSite(serverComment, serverBindings, path);
            

            //DirectoryEntry webServer = new DirectoryEntry(String.Format("IIS://localhost/w3svc/{0}", siteId));
            //webServer.Properties["SecureBindings"].Add(":443:zzz.cordonco.com");
            //webServer.CommitChanges();
            site.SetBindings(serverBindings);

            
            //SetASPNetVersion(w3svc);
            DirectoryEntry virDir = new DirectoryEntry(String.Format("IIS://localhost/w3svc/{0}/root", site.SiteId));
            virDir.Properties["DefaultDoc"].Value = "index.aspx";
            virDir.Properties["AccessFlags"].Value = AccessFlags.AccessRead | AccessFlags.AccessExecute;

            virDir.Properties["AuthFlags"].Value = AuthFlags.AuthNTLM | AuthFlags.AuthAnonymous;

            virDir.Properties["AppPoolId"].Value = appPool;
            ScriptMapper.SetASPNetVersion(virDir);
            virDir.CommitChanges();

            //webServer.Invoke("Start", null);
            site.Start();
        }



        private void button2_Click(object sender, EventArgs e)
        {
            string siteId = string.Empty;

            DirectoryEntry iis = new DirectoryEntry("IIS://localhost/W3SVC");

            String id = "";

            IISWMIHelper.TryGetSiteID(new IISServerCommentIdentifier("zzz"), ref id);

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
