using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IISUtil
{
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
    }

    public enum AspDotNetVersion
    {
        //These values MUST match the values in AspDotNetVersionConst.  This approach might be a bad programming practice
        AspNetV1, AspNetV11, AspNetV2, AspNetV4
    }

}
