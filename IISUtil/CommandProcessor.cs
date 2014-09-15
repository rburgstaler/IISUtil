using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace IISUtil
{

    public delegate void MsgOut(String msg);
    public class CommandProcessor
    {
        public MsgOut ErrorOut { get; set; }
        public MsgOut StatusOut { get; set; }

        private void OutputError(String msg)
        {
            if (ErrorOut != null) ErrorOut(msg);
        }

        private void OutputError(String msg, params Object[] par)
        {
            OutputError(String.Format(msg, par));
        }

        private void OutputStatus(String msg)
        {
            if (StatusOut != null) StatusOut(msg);
        }

        private void OutputStatus(String msg, params Object[] par)
        {
            OutputStatus(String.Format(msg, par));
        }

        public void Run(String[] CmdArguments)
        {
            try
            {
                //We do not want to run if there are invalid arguments... otherwise the end user
                //will think that it ran with success when it did not
                String[] invalids = CommandLineParamsParser.GetInvalidParams(CmdArguments, typeof(CommandParams));
                if (invalids.Length > 0)
                {
                    OutputError("Invalid argument(s) were found.");
                    foreach (String arg in invalids) OutputError("{0} is an invalid argument", arg);
                    OutputError("Fix this before continuing.");
                    return;
                }


                //Make sure that the user did in fact pass in some valid params
                CommandParams cp = new CommandParams();
                if (!CommandLineParamsParser.PopulateParamObject(CmdArguments, cp))
                {
                    OutputError("There were no valid arguments passed in.  Use --Help to determine all valid arguments.");
                    return;
                }

                OutputStatus(String.Format("w3wp (IIS) version: {0}", IIS.Version.FileVersion));

                if (cp.Help != null)
                {
                    String hlp = String.Join(Environment.NewLine, DocHelp.GenerateHelp(typeof(CommandParams)));
                    OutputStatus(hlp);
                    return;
                }

                if (cp.GetInstalledCertificates != null)
                {
                    SSLCertificates.GetInstalledCertificates(OutputStatus);
                }

                //First we want to check if we need to delete a site
                if (cp.DeleteSite != null)
                {
                    if (IIS.Tools.DeleteSite(new IISServerCommentIdentifier(cp.DeleteSite))) //returns true if the site is found
                        OutputStatus("Site {0} deleted", cp.DeleteSite);
                    else
                        OutputStatus("Site {0} not deleted because it was not found", cp.DeleteSite);  //does not warrant an error because that was the desired outcome

                    //Exit out if we are not finding by site id or creating a site here
                    if ((cp.CreateSite == null) && (cp.FindByServerComment == null)) return;
                }

                IISSite site = null;
                //Check if we need to create a new site
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
                    try
                    {
                        site = IIS.Tools.CreateNewSite(new IISServerCommentIdentifier(cp.CreateSite), cp.Bindings ?? "", cp.PhysicalPath);
                        OutputStatus("Site {0} created", cp.CreateSite);
                    }
                    catch (Exception exp)
                    {
                        OutputError("Error creating site {0}: {1}", cp.CreateSite, exp.Message);
                    }
                }

                //If the find parameter is specified, it will override the site that may have been created
                if (cp.FindByServerComment != null)
                {
                    site = IIS.Tools.FindSite(new IISServerCommentIdentifier(cp.FindByServerComment));
                    if (site == null)
                    {
                        OutputError(String.Format("Unable to find site \"{0}\" by server comment.", cp.FindByServerComment));
                        return;
                    }
                    else
                    {
                        OutputStatus("Found site {0} with id {1}", cp.FindByServerComment, site.SiteId);
                    }
                }

                //At this time if we do not have a site object... then we cannot do anything
                if (site == null)
                {
                    OutputError("Unable to create or find a site.  Nothing can be done until proper CreateSite or FindByXXXXX parameters have been specified.");
                    return;
                }

                if (cp.Bindings != null)
                {
                    try
                    {
                        site.SetBindings(cp.Bindings);
                        OutputStatus("Bindings set to {0}", cp.Bindings);
                    }
                    catch (Exception exp)
                    {
                        OutputError(String.Format("Error while setting bindings. {0}", exp.Message));
                        return;
                    }
                }
                if (cp.DefaultDoc != null)
                {
                    site.DefaultDoc = cp.DefaultDoc;
                    OutputStatus("Default document set to {0}", cp.DefaultDoc);
                }



                if (cp.AccessFlags != null)
                {
                    try
                    {
                        site.AccessFlags = CommandLineParamsParser.BuildFlagFromDelimString(cp.AccessFlags, typeof(AccessFlags));
                        OutputStatus("AccessFlags set to {0}", cp.AccessFlags);
                    }
                    catch (Exception exp)
                    {
                        OutputError(String.Format("Error while setting AccessFlags. {0}", exp.Message));
                        return;
                    }
                }
                if (cp.AuthFlags != null)
                {
                    try
                    {
                        site.AuthFlags = CommandLineParamsParser.BuildFlagFromDelimString(cp.AuthFlags, typeof(AuthFlags));
                        OutputStatus("AuthFlags set to {0}", cp.AuthFlags);
                    }
                    catch (Exception exp)
                    {
                        OutputError(String.Format("Error while setting AuthFlags. {0}", exp.Message));
                        return;
                    }
                }

                if (cp.AppPoolId != null)
                {
                    site.AppPoolId = cp.AppPoolId;
                    OutputStatus("AppPoolId set to {0}", cp.AppPoolId);
                }
                if (cp.ASPDotNetVersion != null)
                {
                    AspDotNetVersion version;
                    try
                    {
                        version = (AspDotNetVersion)Enum.Parse(typeof(AspDotNetVersion), cp.ASPDotNetVersion, true);
                    }
                    catch (Exception exp)
                    {
                        OutputError(String.Format("An invalid ASPDotNetVersion value was specified. \"{0}\" is invalid.", cp.ASPDotNetVersion));
                        return;
                    }
                    site.SetASPDotNetVersion(version);
                    OutputStatus("ASP DotNet version set to {0}", version);
                }
                if (cp.StartSite != null)
                {
                    try
                    {
                        site.Start();
                        OutputStatus("Site started with success");
                    }
                    catch (Exception exp)
                    {
                        OutputError("Error starting site: "+exp.Message);
                        return;
                    }
                }
            }
            catch (Exception exp)
            {





                OutputError("An exception took place during execution: " + exp.Message + exp.StackTrace);
            }

        }
    }
}
