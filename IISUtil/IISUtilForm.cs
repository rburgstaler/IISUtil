using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.DirectoryServices;
using System.IO;
using System.Diagnostics;
using Microsoft.Web.Administration;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;

namespace IISUtil
{
    public partial class IISUtilForm : Form
    {
        public IISUtilForm()
        {
            InitializeComponent();
        }

        private void btRun_Click(object sender, EventArgs e)
        {
            String cmdText = tbArguments.Text.Replace(Environment.NewLine, " ");
            String[] args = CommandLineParser.GetArguments(cmdText);
            ProcessArguments(args); 
        }

        private void MinimumAspDotNet4onIIS6ConfigExample()
        {
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

        private void ProcessArguments(String[] CmdArguments)
        {
            CommandParams cp = new CommandParams();
            if (!CommandLineParamsParser.PopulateParamObject(CmdArguments, cp)) return;

            CommandProcessor proc = new CommandProcessor();
            proc.ErrorOut = OutputError;
            proc.StatusOut = OutputStatus;
            proc.Run(cp);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string w3wpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"inetsrv\w3wp.exe");
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(w3wpPath);
            OutputStatus(String.Format("w3wp (IIS) version: {0}", versionInfo.FileVersion));

            //If at least one of the values that we need exists... then we will assume that the user wants this run as a command line tool.
            //but this needs to be reworked to handle the determination differently... Wasted processing as it is because it does
            //PopulateParamObject here and in ProcessArguments().  We need to do determine a good sound way of handling the following scenarios.
            //1.) Invalid parameters when being run from the command line --right now-->uses the valid params if they exist --should-->complain that there was bad params
            //2.) Invalid parameters when being run from the form portion of the app --right now-->uses the valid params if they exist --should-->complain that there was bad params
            //3.) Valid parameters when being run from the command line  --right now--> this is how it determines that it does not want to show the form
            //4.) Valid parameters when being run from the form portion of the app --right now--> will just run
            //----->So... we need to change the PopulateParamObject() method to a valid params method that lets us know which of the 4 scenarios above applies
            CommandParams cp = new CommandParams();
            if (!CommandLineParamsParser.PopulateParamObject(Environment.GetCommandLineArgs(), cp)) return;


            ProcessArguments(Environment.GetCommandLineArgs());
            CommandProcessor proc = new CommandProcessor();
            proc.ErrorOut = OutputError;
            proc.StatusOut = OutputStatus;
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

        private void btGetPossibleArguments_Click(object sender, EventArgs e)
        {
            PropertyInfo[] fis = typeof(CommandParams).GetProperties();
            tbArguments.Text = String.Join(Environment.NewLine, fis.Select(t => "-" + t.Name).ToArray());
        }
    }

}
