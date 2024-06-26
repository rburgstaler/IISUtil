﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using IISUtilLib;
using ACMEClientLib;

namespace IISUtilLib
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

        //Returns true if one of the parameters specified will require the ability to lookup a site
        public static bool SiteIDRequired(Object obj)
        {
            bool retVal = false;
            PropertyInfo[] pos = obj.GetType().GetProperties();
            foreach (PropertyInfo pi in pos)
            {
                List<SiteIDRequiredAttribute> siteIDRequired = pi.GetCustomAttributes(typeof(SiteIDRequiredAttribute), true).Cast<SiteIDRequiredAttribute>().ToList();
                retVal = (pi.GetValue(obj) != null) && (siteIDRequired.Count > 0);
                if (retVal) break;
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
        [Documentation("Get the web site that is bound to the specified binding. Example binding string:  http:*:80:www.abcdefg.com or https:*:443:www.abcdefg.com:CertStoreName\\a03083aabcd6bdfec92214df7e885c9e1e1a864d")]
        public String FindByBinding { get; set; }
        [Documentation("Delete the site specified by the following parameter")]
        [SiteIDRequired]
        public String DeleteSite { get; set; }
        [Documentation("Create a new site with the following parameter.")]
        public String CreateSite { get; set; }
        [SiteIDRequired]
        [Documentation("Set the file path to the site.")]
        public String PhysicalPath { get; set; }
        [Documentation("Set the bindings on the selected site(s).  Example binding: Example binding string:  http:*:80:www.abcdefg.com or https:*:443:www.abcdefg.com:CertStoreName\\a03083aabcd6bdfec92214df7e885c9e1e1a864d")]
        public String Bindings { get; set; }
        [SiteIDRequired]
        public String DefaultDoc { get; set; }
        [Documentation("\"|\" sperated list that is used to specify Access Flags")]
        [ValidValuesAttribute(typeof(AccessFlags))]
        [SiteIDRequired]
        public String AccessFlags { get; set; }
        [Documentation("\"|\" sperated list that is used to specify Authorization Flags")]
        [ValidValuesAttribute(typeof(AuthFlags))]
        [SiteIDRequired]
        public String AuthFlags { get; set; }
        [SiteIDRequired]
        [Documentation("Specify the AppPoolId to use for the newly setup web app.")]
        public String AppPoolId { get; set; }
        [Documentation("Setup the version of .NET to use. [Examples: AspNetV1, AspNetV11, AspNetV2, AspNetV4]")]
        public String ASPDotNetVersion { get; set; }
        [Documentation("Start the site currently being operated on")]
        public String StartSite { get; set; }
        [Documentation("Display parameter help")]
        public String Help { get; set; }
        [Documentation("Display all certificate hashes as well as the corresponding names")]
        public String GetInstalledCertificates { get; set; }
        [Documentation("Display info on all sites.")]
        public String GetAllSites { get; set; }
        [Documentation("Location to dump all ACME intermediate files to.")]
        public String ACMEv2ConfigPath { get; set; } = "";
        [Documentation("ACME location to use.  Examples: https://acme-staging-v02.api.letsencrypt.org/, https://acme-v02.api.letsencrypt.org/")]
        public String ACMEv2BaseUri { get; set; } = "";
        [Documentation("Email associate with requester of ACME cert")]
        public String ACMEv2SignerEmail { get; set; } = "";
        [Documentation("DNS requesting to create cert for")]
        public String ACMEv2DnsIdentifiers { get; set; } = "";
        [Documentation("Path to put the resulting certs to")]
        public String ACMEv2CertificatePath { get; set; } = "";
        [Documentation("Filename of the pfx to install into the Windows Credential store.")]
        public String InstallCertPFXFileName { get; set; } = "";
        [Documentation("Password of the pfx to install into the Windows Credential store.")]
        public String InstallCertPFXPassword { get; set; } = "";
        [Documentation("Specify the Windows Certificate store to store the cert to.  Will default to WebHosting if it is left blank.")]
        public String InstallCertStore { get; set; } = "";
        [Documentation("DNS query to filter out out hosts in -GetAllSites and -UpdateBindingCert. Examples: *.contoso.com, *.bla.consoto.com, bla.bla.contoso.com")]
        public String DNSQuery { get; set; }
        [Documentation("Update all SSL sites that match the DNSQuery parameter and set the corresponding cert store and cert hash (This is the SHA1 hash of the cert).  Example value: WebHosting\\7AB5E888366D3615778B3A56AB0E1B3AED44909F")]
        public String UpdateBindingCert { get; set; }
        [Documentation("Return JSON object with info about the cert.  Currently only supports pfx.")]
        public String GetCertInfoFileName { get; set; }
        [Documentation("Password of cert file from /CertInfoFileName parameter.")]
        public String GetCertInfoPassword { get; set; }
    }

    public class SiteIDRequiredAttribute : Attribute
    {
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
