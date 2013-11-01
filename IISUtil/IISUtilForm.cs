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
            string appPool = "DotNet4AppPool";


            Directory.CreateDirectory(path);
            IISWMISite site = IISWMISite.CreateNewSite(serverComment, serverBindings, path);
            site.SetBindings(serverBindings);
            site.DefaultDoc = "index.aspx";
            site.AccessFlags = AccessFlags.AccessRead | AccessFlags.AccessExecute;
            site.AuthFlags = AuthFlags.AuthNTLM | AuthFlags.AuthAnonymous;
            site.AppPoolId = appPool;
            site.SetASPDotNetVersion(AspDotNetVersion.AspNetV4);
            site.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IISWMISite.DeleteSite(new IISServerCommentIdentifier("zzz"));
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
