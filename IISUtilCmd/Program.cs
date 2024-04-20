using IISUtilLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISUtilCmd
{
    internal class IISUtilCmdProgram
    {
        static void Main(string[] args)
        {
            CommandProcessor proc = new CommandProcessor();
            proc.ErrorOut = Console.Error.WriteLine;
            proc.StatusOut = Console.Out.WriteLine;
            proc.Run(args);
        }
    }
}