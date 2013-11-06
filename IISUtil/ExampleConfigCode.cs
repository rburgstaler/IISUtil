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
            string serverBindings = "http::80:zzz.cordonco.com;https::443:zzz.cordonco.com";
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
    }
}
