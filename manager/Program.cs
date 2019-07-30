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
            var fxLib = IntPtr.Size == 8 ? "Firefox64" : "Firefox86";
            Gecko.Xpcom.Initialize(fxLib);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MngForm());
            Application.Run(new MngForm());
        }
    }
}
