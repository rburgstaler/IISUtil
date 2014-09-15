using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IISUtil
{
    public class CommandLineParamsParser
    {
        public static Boolean ParamExists(String[] sArgs, String AOption)
        {
            String dummy = "";
            return GetParam(sArgs, AOption, ref dummy);

        }

        //Finds all arguments that qualify as parameter name but are not in the parameter object...  An empty array result is good!
        public static String[] GetInvalidParams(String[] sArgs, Type paramObjType)
        {
            List<String> retList = new List<String>();
            String paramName = "";
            for (int x = 0; x < sArgs.Length; x++)
            {
                if (TryGetParamName(sArgs[x], ref paramName))
                {
                    PropertyInfo fi = paramObjType.GetProperties().FirstOrDefault(t => t.Name.Equals(paramName, StringComparison.CurrentCultureIgnoreCase));
                    if (fi == null) retList.Add(sArgs[x]);
                }
            }
            return retList.ToArray();
        }

        private static Regex ParamReg = new Regex(@"^(-|/|--)(\w+)");  //I am guessing this is not thread safe (keep that in mind)
        //Returns true if the argument is a valid param name and returns the paramname
        public static Boolean TryGetParamName(String Argument, ref String paramName)
        {
            MatchCollection mc = ParamReg.Matches(Argument);
            bool retVal = (mc.Count >0) && (mc[0].Groups.Count>2);
            if (retVal) paramName = mc[0].Groups[2].Value;
            return retVal;

        }
        
        public static Boolean GetParam(String[] sArgs, String AOption, ref String AParamValue)
        {
            String prefixedOption = AOption;
            AParamValue = "";
            String paramName = "";
            for (int x = 0; x < sArgs.Length; x++)
            {
                if ((TryGetParamName(sArgs[x], ref paramName)) && (paramName.Equals(AOption, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if ((x + 1) < sArgs.Length) AParamValue = sArgs[x + 1];
                    //The result is true whether there is a corresponding value or not
                    return true;
                }

            }
            return false;
        }

        //Returns true if at least one value got populated
        public static bool PopulateParamObject(String[] sArgs, Object obj)
        {
            bool retVal = false;
            PropertyInfo[] pos = obj.GetType().GetProperties();
            String paramVal = "";
            foreach (PropertyInfo pi in pos)
            {
                if (GetParam(sArgs, pi.Name, ref paramVal))
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
                FieldInfo fi = flagObjType.GetFields().FirstOrDefault(t => t.Name.Equals(flagVal, StringComparison.CurrentCultureIgnoreCase));
                if (fi == null) throw new Exception(String.Format("Flag value {0} is invalid.", flagVal));
                retVal = retVal | (Int32)fi.GetValue(null);
            }
            return retVal;
        }
    }



    public class CommandParams
    {
        [Documentation("Get the web site to operate on based on the server comment (also known as the name)")]
        public String FindByServerComment { get; set; }
        [Documentation("Delete the site specified by the following parameter")]
        public String DeleteSite { get; set; }
        public String CreateSite { get; set; }
        public String PhysicalPath { get; set; }
        public String Bindings { get; set; }
        public String DefaultDoc { get; set; }
        [Documentation("\"|\" sperated list that is used to specify Access Flags")]
        [ValidValuesAttribute(typeof(AccessFlags))]
        public String AccessFlags { get; set; }
        [Documentation("\"|\" sperated list that is used to specify Authorization Flags")]
        [ValidValuesAttribute(typeof(AuthFlags))]
        public String AuthFlags { get; set; }
        public String AppPoolId { get; set; }
        public String ASPDotNetVersion { get; set; }
        [Documentation("Start the site currently being operated on")]
        public String StartSite { get; set; }
        [Documentation("Display parameter help")]
        public String Help { get; set; }
        [Documentation("Display all certificate hashes as well as the corresponding names")]
        public String GetInstalledCertificates { get; set; }
    }

    public class ValidValuesAttribute : Attribute
    {
        public Type OptionsObjectType { get; set; }
        public ValidValuesAttribute(Type tp)
        {
            OptionsObjectType = tp;
        }
    }
        
    public class DocumentationAttribute : Attribute
    {
        public String Description { get; set; }
        public DocumentationAttribute(String desc)
        {
            Description = desc;
        }
    }

    public class DocHelp
    {
        public static String[] GenerateHelp(Type objType)
        {
            List<String> ls = new List<String>();
            PropertyInfo[] pos = objType.GetProperties();
            foreach (PropertyInfo pi in pos)
            {
                ls.Add("/"+pi.Name);
                List<DocumentationAttribute> docs = pi.GetCustomAttributes(typeof(DocumentationAttribute), true).Cast<DocumentationAttribute>().ToList();
                List<ValidValuesAttribute> options = pi.GetCustomAttributes(typeof(ValidValuesAttribute), true).Cast<ValidValuesAttribute>().ToList();
                ls.AddRange(docs.Select(t => "  "+t.Description));

                foreach (ValidValuesAttribute att in options)
                    ls.Add("  Valid options: " + String.Join("|", att.OptionsObjectType.GetFields().Select(t => t.Name)));
            }
            return ls.ToArray();
        }

    }
}
