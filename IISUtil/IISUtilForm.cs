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
using System.Drawing;

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

        private void ProcessArguments(String[] CmdArguments)
        {
            CommandProcessor proc = new CommandProcessor();
            proc.ErrorOut = OutputError;
            proc.StatusOut = OutputStatus;
            proc.Run(CmdArguments);
        }

        private String StoreFile
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Store.txt");
            }        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(StoreFile)) tbArguments.Text = File.ReadAllText(StoreFile);

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

            Close();
            // When using a winforms app with AttachConsole the app complets but there is no newline after the process stops. 
            //This gives the newline and looks normal from the console:
            SendKeys.SendWait("{ENTER}");

        }


        public void OutputError(String errorMessage)
        {
            AppendOuputText(errorMessage + Environment.NewLine, Color.Red);
            Console.Error.WriteLine(errorMessage);
        }
        public void OutputStatus(String statusMessage)
        {
            AppendOuputText(statusMessage + Environment.NewLine, Color.Black);
            Console.WriteLine(statusMessage);
        }

        private void btGetPossibleArguments_Click(object sender, EventArgs e)
        {
            PropertyInfo[] fis = typeof(CommandParams).GetProperties();
            tbArguments.Text = String.Join(Environment.NewLine, fis.Select(t => "-" + t.Name).ToArray());
        }

        private void IISUtilForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            File.WriteAllText(StoreFile, tbArguments.Text);
        }

        private void AppendOuputText(string text, Color color)
        {
            tbOutput.SelectionStart = tbOutput.TextLength;
            tbOutput.SelectionLength = 0;

            tbOutput.SelectionColor = color;
            tbOutput.AppendText(text);
            tbOutput.SelectionColor = tbOutput.ForeColor;
        }

        String siteName = "GitWebApp";
        private void button1_Click(object sender, EventArgs e)
        {

            String path = String.Format(@"D:\Dev\Git-Web-Access\GitTools.WebApp", siteName);
            //Get caught up with the IIS7-8.5 ServerManager so that we can start integrating that bad boy in
            ServerManager serverMgr = new ServerManager();

            if (serverMgr.Sites[siteName] != null)
            {
                OutputError(String.Format("Site {0} already exists.", siteName));
                return;
            }

            Directory.CreateDirectory(path);
            Site mySite = serverMgr.Sites.Add(siteName, path, 80);
            //Site mySite = serverMgr.Sites.Add(siteName, "http", "http:*:80:dadada.burgstaler.com", path);
            mySite.Bindings.Clear();
            Microsoft.Web.Administration.Binding binding = mySite.Bindings.CreateElement("binding");
            binding.Protocol = "http";
            binding.BindingInformation = String.Format("*:80:{0}.burgstaler.com", siteName);
            mySite.Bindings.Add(binding);
            //mySite.Bindings.Add("http:*:80:ddd.burgstaler.com", "http");
            ApplicationPool appPool = serverMgr.ApplicationPools[siteName];
            appPool = appPool ?? serverMgr.ApplicationPools.Add(siteName);
            appPool.ManagedRuntimeVersion = "v4.0"; //v1.0, v2.0
            mySite.ApplicationDefaults.ApplicationPoolName = siteName;
            //mySite.TraceFailedRequestsLogging.Enabled = true;
            //mySite.TraceFailedRequestsLogging.Directory = "C:\\inetpub\\customfolder\\site";
            serverMgr.CommitChanges();

            //Start will report an error "The object identifier does not represent a valid object. (Exception from 
            //HRESULT: 0x800710D8)" if we don't give some time as mentioned by Sergei - http://forums.iis.net/t/1150233.aspx
            //There is a timing issue. WAS needs more time to pick new site or pool and start it, therefore (depending on your system) you could 
            //see this error, it is produced by output routine. Both site and pool are succesfully created, but State field of their PS 
            //representation needs runtime object that wasn't created by WAS yet.
            //He said that would be fixed soon, but apparently that did not take place yet so we will work around it.
            DateTime giveUpAfter = DateTime.Now.AddSeconds(3);
            while (true)
            {
                try
                {
                    mySite.Start();
                    break;
                }
                catch (Exception exp)
                {
                    if (DateTime.Now > giveUpAfter)
                    {
                        //throw new Exception(String.Format("Inner error: {0} Outer error: {1}", (exp.InnerException != null) ? exp.InnerException.Message : "No inner exception", exp.Message));
                        OutputError(String.Format("Inner error: {0} Outer error: {1}", (exp.InnerException != null) ? exp.InnerException.Message : "No inner exception", exp.Message));
                        break;
                    }
                }
                System.Threading.Thread.Sleep(250);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ServerManager serverMgr = new ServerManager();
            Site s1 = serverMgr.Sites[siteName]; // you can pass the site name or the site ID
            if (s1 != null)
            {
                s1.Stop();
                serverMgr.Sites.Remove(s1);
                serverMgr.CommitChanges();
            }
            ApplicationPool appPool = serverMgr.ApplicationPools[siteName];
            if (appPool != null)
            {
                serverMgr.ApplicationPools.Remove(appPool);
                serverMgr.CommitChanges();
            }

        }

    }

}
