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
            string serverBindings = "http::80:zzz.cordonco.com;https::443:zzz.cordonco.com";
            string appPool = "DotNet4AppPool";


            Directory.CreateDirectory(path);
            IISWMISite site = IISWMISite.CreateNewSite(serverComment, serverBindings, path);
            site.SetBindings(serverBindings);
            site.DefaultDoc = "index.aspx";
            site.AccessFlags = AccessFlags.AccessRead | AccessFlags.AccessExecute;
            site.AuthFlags = AuthFlags.AuthNTLM | AuthFlags.AuthAnonymous;
            site.AppPoolId = appPool;
            site.SetASPDotNetVersion(AspDotNetVersion.AspNetV4);
            try
            {
                site.Start();
            }
            catch (Exception exp)
            {
                OutputError(exp.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IISWMISite.DeleteSite(new IISServerCommentIdentifier("zzz"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string w3wpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"inetsrv\w3wp.exe");
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(w3wpPath);
            OutputStatus(String.Format("w3wp (IIS) version: {0}", versionInfo.FileVersion));

            //If at least one of the values that we need exists... then we will assume that the user wants this run as a command line tool.
            CommandParams cp = new CommandParams();
            if (!CommandLineParamsParser.PopulateParamObject(cp)) return;

            CommandProcessor proc = new CommandProcessor();
            proc.ErrorOut = OutputError;
            proc.Run(cp);


            Close();
            // When using a winforms app with AttachConsole the app complets but there is no newline after the process stops. 
            //This gives the newline and looks normal from the console:
            SendKeys.SendWait("{ENTER}");

        }


        public void OutputError(String errorMessage)
        {
            textBox1.Text += errorMessage + Environment.NewLine;
            Console.Error.WriteLine(errorMessage);
        }
        public void OutputStatus(String statusMessage)
        {
            textBox1.Text += statusMessage + Environment.NewLine;
            Console.WriteLine(statusMessage);
        }
    }

}
