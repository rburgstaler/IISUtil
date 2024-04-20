using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IISUtilLib
{
    public class CommandLineParser
    {
        //Thanks to Thomas Petersson: http://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp 
        public static String[] GetArguments(String CmdLine)
        {
            //Does not work properly with leading space
            String cmdtxt = CmdLine.Trim();
            String re = @"\G(""((""""|[^""])+)""|(\S+)) *";
            MatchCollection ms = Regex.Matches(cmdtxt, re);
            return ms.Cast<Match>().Select(m => Regex.Replace(m.Groups[2].Success ? m.Groups[2].Value : m.Groups[4].Value, @"""""", @"""")).ToArray();

        }
    }
}
