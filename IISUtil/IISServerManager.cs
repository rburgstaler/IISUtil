using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;
using System.IO;
using System.Reflection;

namespace IISUtil
{
    public class IISSM : IIS
    {
        public override IISSite CreateNewSite(IISServerCommentIdentifier serverComment, String serverBindings, String filePath)
        {
            return IISServerManagerSite.CreateNewSite(serverComment.Value, serverBindings, filePath);
        }
        public override bool DeleteSite(IISIdentifier siteIdentifier)
        {
            return IISServerManagerSite.DeleteSite(siteIdentifier);
        }
        public override IISSite FindSite(IISIdentifier siteIdentifier)
        {
            return IISServerManagerSite.FindSite(siteIdentifier);
        }
    }

    public class IISServerManagerSite : IISSite
    {
        public static IISSite FindSite(IISIdentifier Identifier)
        {
            ServerManager sm = new ServerManager();
            IISServerManagerSite retVal = new IISServerManagerSite();
            retVal.site = sm.Sites[Identifier.Value];
            //Return null if the site was not found
            return (retVal.site != null) ? retVal : null;
        }

        public static bool DeleteSite(IISIdentifier siteIdentifier)
        {
            if (!(siteIdentifier is IISServerCommentIdentifier)) throw new Exception(String.Format("Identifier not yet supported {}", siteIdentifier.GetType().Name));
            ServerManager sm = new ServerManager();
            Site site = sm.Sites[siteIdentifier.Value];
            if (site == null) return false;
            //if this site is in a bad state, the following calls will fail so we will just trap the exception for now
            try { site.Stop(); }
            catch { }
            sm.Sites.Remove(site);
            sm.CommitChanges();
            return true;
        }

        ServerManager ServerMgr = new ServerManager();
        Site site;
        public static IISServerManagerSite CreateNewSite(String serverComment, String serverBindings, String filePath)
        {
            Directory.CreateDirectory(filePath);
            IISServerManagerSite retVal = new IISServerManagerSite();
            retVal.site = retVal.ServerMgr.Sites.Add(serverComment, filePath, 80);
            retVal.SetBindings(serverBindings);

            //We also need to setup an app pool most likely
            ApplicationPool appPool = retVal.ServerMgr.ApplicationPools[serverComment];
            appPool = appPool ?? retVal.ServerMgr.ApplicationPools.Add(serverComment);
            retVal.site.ApplicationDefaults.ApplicationPoolName = serverComment;

            return retVal;
        }

        public void CommitServerManagerChanges()
        {
            //Anytime we commit changes, the ServerMgr becomes read-only.  To make it writable again, create a new instance.
            ServerMgr.CommitChanges();
            ServerMgr = new ServerManager();
            site = ServerMgr.Sites[site.Name];
        }

        ////Return null if the site is not to be found
        //public static IISWMISite FindSite(IISIdentifier Identifier)
        //{
        //    String id = "";
        //    //need to be sure that the site exists or else it can throw an error
        //    if (IISWMIHelper.TryGetSiteID(Identifier, ref id))
        //    {
        //        return new IISWMISite()
        //        {
        //            SiteId = id
        //        };
        //    }
        //    return null;
        //}

        ////http:*:80:www.abcdefg.com
        ////https:*:443:www.abcdefg.com
        public override void SetBindings(String siteBindings)
        {
            site.Bindings.Clear();
            //We need to parse the bindings string
            IISBindingParser.Parse(siteBindings,
                delegate(IISBinding iisBinding)
                {
                    Microsoft.Web.Administration.Binding binding = site.Bindings.CreateElement("binding");
                    binding.Protocol = iisBinding.Protocol;
                    binding.BindingInformation = iisBinding.SMBindString;
                    site.Bindings.Add(binding);
                });
            CommitServerManagerChanges();
        }

        public override void Start()
        {
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
                    site.Start();
                    break;
                }
                catch (Exception exp)
                {
                    if (DateTime.Now > giveUpAfter)
                    {
                        throw new Exception(String.Format("Inner error: {0} Outer error: {1}.  \r\n{2}", (exp.InnerException != null) ? exp.InnerException.Message : "No inner exception", exp.Message, exp.StackTrace));
                        break;
                    }
                }
                System.Threading.Thread.Sleep(250);
            }
        }
        public override String SiteId 
        {
            get
            {
                return site.Id.ToString();
            }
            set
            {
                //Not implemented yet
            }
        }


        //private T GetVirtualDirPropertyDef<T>(String propertyName, T DefaultValue)
        //{
        //    PropertyValueCollection pc = IISWMIHelper.GetIIsWebVirtualDir(SiteId).Properties["propertyName"];
        //    return (pc != null) ? (T)Convert.ChangeType(pc.Value, typeof(T)) : DefaultValue;
        //}
        //private void SetVirtualDirProperty(String propertyName, object propertyValue)
        //{
        //    DirectoryEntry virDir = IISWMIHelper.GetIIsWebVirtualDir(SiteId);
        //    virDir.Properties[propertyName].Value = propertyValue;
        //    virDir.CommitChanges();
        //}

        public override String DefaultDoc
        {
            get
            {
                return "";  //Not implemented yet
            }
            set
            {
                //Not implemented yet
            }
        }

        public override String AppPoolId
        {
            get
            {
                return site.ApplicationDefaults.ApplicationPoolName;
            }
            set
            {
                //First... if the specified application pool does not exist, then create one
                ApplicationPool appPool = ServerMgr.ApplicationPools[value];
                if (appPool==null) ServerMgr.ApplicationPools.Add(value);

                site.ApplicationDefaults.ApplicationPoolName = value;
                CommitServerManagerChanges();
            }
        }

        public override Int32 AccessFlags
        {
            get
            {
                return 0;
            }
            set
            {
                //Not implemented yet
            }
        }

        public override Int32 AuthFlags
        {
            get
            {
                return 0;
            }
            set
            {
                //Not implemented yet
            }
        }

        public override void SetASPDotNetVersion(AspDotNetVersion version)
        {
            ApplicationPool appPool = ServerMgr.ApplicationPools[site.ApplicationDefaults.ApplicationPoolName];
            appPool.ManagedRuntimeVersion = AspDotNetServerManagerVersionConst.VersionString(version);
            CommitServerManagerChanges();
        }
    }


    public static class AspDotNetServerManagerVersionConst
    {
        public const string AspNetV1 = "v1.0";
        public const string AspNetV11 = "v1.0";
        public const string AspNetV2 = "v2.0";
        public const string AspNetV4 = "v4.0";

        public static String VersionString(AspDotNetVersion version)
        {
            FieldInfo fi = typeof(AspDotNetServerManagerVersionConst).GetField(version.ToString());
            return (fi == null) ? "" : Convert.ToString(fi.GetValue(null));
        }
    }
}
