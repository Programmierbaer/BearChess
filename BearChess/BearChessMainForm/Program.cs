using System;
using System.Windows.Forms;

namespace www.SoLaNoSoft.com.BearChessMainForm
{
    internal static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BearChessMainForm());
        }
    }
}
