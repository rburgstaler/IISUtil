using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public void Run(CommandParams cp)
        {
            try
            {
                //First we want to check if we need to delete a site
                if (cp.DeleteSite != null)
                {
                    IISWMISite.DeleteSite(new IISServerCommentIdentifier(cp.DeleteSite));
                    return;
                }

                IISWMISite site = null;
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
                    site = IISWMISite.CreateNewSite(cp.CreateSite, cp.Bindings ?? "", cp.PhysicalPath);
                }

                //If the find parameter is specified, it will override the site that may have been created
                if (cp.FindByServerComment != null)
                {
                    site = IISWMISite.FindSite(new IISServerCommentIdentifier(cp.FindByServerComment));
                    if (site == null)
                    {
                        OutputError(String.Format("Unable to find site. {0]", cp.FindByServerComment));
                        return;
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
                }



                if (cp.AccessFlags != null)
                {
                    try
                    {
                        site.AccessFlags = CommandLineParamsParser.BuildFlagFromDelimString(cp.AccessFlags, typeof(AccessFlags));
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
                        site.AccessFlags = CommandLineParamsParser.BuildFlagFromDelimString(cp.AuthFlags, typeof(AuthFlags));
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
                }
                if (cp.StartSite != null)
                {
                    try
                    {
                        site.Start();
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
