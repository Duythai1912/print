using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LippsPrinter.Utils
{
    internal class Bk
    {
        public static void PrintUsingAdobeAcrobat(string fullFilePathForPrintProcess, string printerName)
        {
            string printApplicationPath = Microsoft.Win32.Registry.LocalMachine
            .OpenSubKey("Software")
            .OpenSubKey("Microsoft")
            .OpenSubKey("Windows")
            .OpenSubKey("CurrentVersion")
            .OpenSubKey("App Paths")
            .OpenSubKey("Acrobat.exe")
            .GetValue(String.Empty).ToString();

            const string flagNoSplashScreen = "/s";
            const string flagOpenMinimized = "/h";

            var flagPrintFileToPrinter = string.Format("/t \"{0}\" \"{1}\"", fullFilePathForPrintProcess, printerName);

            var args = string.Format("{0} {1} {2}", flagNoSplashScreen, flagOpenMinimized, flagPrintFileToPrinter);

            var startInfo = new ProcessStartInfo
            {
                FileName = printApplicationPath,
                Arguments = args,
                CreateNoWindow = true,
                ErrorDialog = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = Process.Start(startInfo);
            process.EnableRaisingEvents = true;


            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.WaitForExit();
                    process.WaitForInputIdle();
                    process.CloseMainWindow();
                }

                process.Kill();
                process.Dispose();
            }
        }
    }
}
