using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace IISUtil
{
    static class Program
    {

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args )
        {
            // redirect console output to parent process;
            // must be before any calls to Console.WriteLine()
            AttachConsole(ATTACH_PARENT_PROCESS);


            // to demonstrate where the console output is going
            int argCount = (args == null) ? 0 : args.Length;
            Console.WriteLine("You specified {0} arguments:", argCount);
            for (int i = 0; i < argCount; i++)
            {
                Console.WriteLine("  {0}", args[i]);
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new IISUtilForm());



        }
    }
}
