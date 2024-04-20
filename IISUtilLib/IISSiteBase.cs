using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace IISConfigLib
{
    public class IISIdentifier
    {
        public String Value { get; set; }
        public IISIdentifier(String ID)
        {
            Value = ID;
        }
    }

    public abstract class IIS
    {
        //The following method determines whether WMI or ServerManager based logic
        //is required.
        private static IIS _IIS = null;
        public static IIS Tools
        {
            get
            {
                if (_IIS == null)
                {
                    if (Version.ProductMajorPart < 8) _IIS = new IISWMI();
                    else _IIS = new IISSM();
                }
                return _IIS;
            }
        }

        public static FileVersionInfo Version
        {
            get
            {
                string w3wpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"inetsrv\w3wp.exe");
                return FileVersionInfo.GetVersionInfo(w3wpPath);
            }
        }

        public abstract IISSite CreateNewSite(IISServerCommentIdentifier serverComment, String serverBindings, String filePath);
        public abstract bool DeleteSite(IISIdentifier siteIdentifier);
        public abstract IISSite FindSite(IISIdentifier siteIdentifier);

    }

    public abstract class IISSite
    {
        //http::80:www.abcdefg.com
        //https::443:www.abcdefg.com
        public abstract void SetBindings(String siteBindings);
        public abstract void Start();
        public abstract String DefaultDoc { get; set; }
        public abstract String AppPoolId { get; set; }
        public abstract Int32 AccessFlags { get; set; }
        public abstract Int32 AuthFlags { get; set; }
        public abstract void SetASPDotNetVersion(AspDotNetVersion version);
        public abstract String SiteId { get; set; }
        public abstract IISBinding FindBinding(IISBinding iisBinding);
    }

    public enum AspDotNetVersion
    {
        //These values MUST match the values in AspDotNetVersionConst.  This approach might be a bad programming practice
        AspNetV1, AspNetV11, AspNetV2, AspNetV4
    }

}
