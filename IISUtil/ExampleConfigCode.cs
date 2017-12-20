using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IISUtil
{
    //This unit contains some sample code that can be used for reference.  It was kept around as a working
    //example of how one could quickly hack a script to do some manipulation of IIS.
    public class ExampleConfigCode
    {
        private static void MinimumAspDotNet4onIIS6ConfigExample(MsgOut msgOut)
        {
            string serverComment = "zzz";
            string path = @"C:\Inetpub\zzz";
            string serverBindings = "http::80:zzz.contoso.com;https::443:zzz.contoso.com";
            string appPool = "DotNet4AppPool";


            Directory.CreateDirectory(path);
            IISSite site = IIS.Tools.CreateNewSite(new IISServerCommentIdentifier(serverComment), serverBindings, path);
            site.SetBindings(serverBindings);
            site.DefaultDoc = "index.aspx";
            site.AccessFlags = AccessFlags.AccessRead | AccessFlags.AccessExecute;
            site.AuthFlags = AuthFlags.AuthNTLM | AuthFlags.AuthAnonymous;
            site.AppPoolId = appPool;
            site.SetASPDotNetVersion(AspDotNetVersion.AspNetV4);
            try
            {
                site.Start();
            }
            catch (Exception exp)
            {
                msgOut(exp.Message);
            }
        }
        
        public static String MinimumAspDotNet4onIIS85ConfigExample_CommandLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"-DeleteSite TestSite08");
            sb.AppendLine(@"-CreateSite TestSite08 -PhysicalPath D:\debug\IISTest\TestSite08");
            sb.AppendLine(@"-DefaultDoc index.html");
            sb.AppendLine(@"-AccessFlags AccessRead|AccessExecute");
            sb.AppendLine(@"-AuthFlags AuthNTLM|AuthAnonymous");
            sb.AppendLine(@"-AppPoolId TestSite08");
            sb.AppendLine(@"-ASPDotNetVersion AspNetV4");
            sb.AppendLine(@"-Bindings http::80:testsite08.internaltest.local");
            sb.AppendLine(@"-StartSite");
            return sb.ToString();
        }


    }
}
