using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace IISUtil
{
    public class CommandLineParamsParser
    {
        public static Boolean ParamExists(String AOption)
        {
            String dummy = "";
            return GetParam(AOption, ref dummy);

        }
        public static Boolean GetParam(String AOption, ref String AParamValue)
        {
            String prefixedOption = AOption;
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

        //Returns true if at least one value got populated
        public static bool PopulateParamObject(Object obj)
        {
            bool retVal = false;
            PropertyInfo[] pos = obj.GetType().GetProperties();
            String paramVal = "";
            foreach (PropertyInfo pi in pos)
            {
                if (GetParam("-" + pi.Name, ref paramVal))
                {
                    pi.SetValue(obj, paramVal, null);
                    retVal = true;
                }
            }
            return retVal;
        }

        //Use a class with static methods on it to build a flag mask based on the fields in the
        //class matching the | split aFlagString values
        public static Int32 BuildFlagFromDelimString(String aFlagString, Type flagObjType)
        {
            Int32 retVal = 0;
            String[] flagValues = aFlagString.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String flagVal in flagValues)
            {
                FieldInfo fi = flagObjType.GetFields().First(t => t.Name.Equals(flagVal, StringComparison.CurrentCultureIgnoreCase));
                if (fi == null) throw new Exception(String.Format("Flag value {0} is invalid.", flagVal));
                retVal = retVal | (Int32)fi.GetValue(null);
            }
            return retVal;
        }
    }

    public class CommandParams
    {
        public String FindByServerComment { get; set; }
        public String DeleteSite { get; set; }
        public String CreateSite { get; set; }
        public String PhysicalPath { get; set; }
        public String Bindings { get; set; }
        public String DefaultDoc { get; set; }
        public String AccessFlags { get; set; }
        public String AuthFlags { get; set; }
        public String AppPoolId { get; set; }
        public String ASPDotNetVersion { get; set; }
        public String StartSite { get; set; }
    }
    /*
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
 
     */
}
