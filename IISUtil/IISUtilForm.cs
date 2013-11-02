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
            OutputStatus(String.Format("w3wp (IIS) version: {0}", versionInfo.FileVersion));

            //If at least one of the values that we need exists... then we will assume that the user wants this run as a command line tool.
            CommandParams cp = new CommandParams();
            if (!CommandLineParamsParser.PopulateParamObject(cp)) return;

            try
            {
                IISWMISite site = null;
                //First thing is to check if we need to create a new site
                if (cp.CreateSite != null)
                {
                    if (cp.CreateSite.Trim() == "")
                    {
                        OutputError("Create site cannot specify a blank site.");
                        return;
                    } 
                    if (String.IsNullOrEmpty(cp.PhysicalPath))
                    {
                        OutputError("In order to create a website, a valid \"PhysicalPath\" must be specified.");
                        return;
                    }
                    site = IISWMISite.CreateNewSite(cp.CreateSite, cp.Bindings ?? "", cp.PhysicalPath);
                }

                //Finish--try to lookup the site based on some input parameters

                //At this time if we do not have a site object... then we cannot do anything
                if (site == null)
                {
                    OutputError("We were unable to create or find a site.  Nothing can be done until proper CreateSite or FindByXXXXX parameters have been specified.");
                    return;
                }

                if (cp.Bindings != null)
                {
                    site.DefaultDoc = cp.DefaultDoc;
                }
                if (cp.DefaultDoc != null)
                {
                    site.DefaultDoc = cp.DefaultDoc;
                }
                if (cp.AccessFlags != null)
                {

                }
                if (cp.AppPoolId != null)
                {
                    site.AppPoolId = cp.AppPoolId;
                }
                if (cp.ASPDotNetVersion != null)
                {
                    AspDotNetVersion version;
                    try
                    {
                        version = (AspDotNetVersion)Enum.Parse(typeof(AspDotNetVersion), cp.ASPDotNetVersion, false);
                    }
                    catch (Exception exp)
                    {
                        OutputError(String.Format("An invalid ASPDotNetVersion value was specified. \"{0}\" is invalid.", cp.ASPDotNetVersion));
                        return;
                    }
                    site.SetASPDotNetVersion(version);

                }
             
        //public String Bindings { get; set; }
        //public String DefaultDoc { get; set; }
        //public String AccessFlags { get; set; }
        //public String AppPoolId { get; set; }
        //public String ASPDotNetVersion { get; set; }

            }
            finally
            {

                Close();
                // When using a winforms app with AttachConsole the app complets but there is no newline after the process stops. 
                //This gives the newline and looks normal from the console:
                SendKeys.SendWait("{ENTER}");
            }
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
