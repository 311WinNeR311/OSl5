using RGiesecke.DllExport;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OS_lab_3_CS_UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [DllExport("Start_Program", CallingConvention = CallingConvention.StdCall)]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
