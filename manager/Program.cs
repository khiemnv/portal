using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_binding
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //set env var LD_LIBRARY_PATH to $(ProjectDir) / Firefox
            //Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", @"D:\tmp\github\portal\manager\Firefox");
            Gecko.Xpcom.Initialize("Firefox");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MngForm());
            Application.Run(new MngForm());
        }
    }
}
