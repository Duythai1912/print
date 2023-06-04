using LippsPrinter.Utils;
using Microsoft.Win32;
using Spire.Pdf;
using System;
using System.Configuration;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LippsPrinter
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            CheckRegistry();
            RenderSetting();
            _ = PrintAsync();
        }


        private void SaveCongfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Minimal);
        }

        private async Task PrintAsync()
        {
            if (Environment.GetCommandLineArgs().Length <= 1) return;
            string defaultPrinter = ConfigurationManager.AppSettings[Constants.DEFAULT_PRINTER];
            if (defaultPrinter == null) return;
            string args = Environment.GetCommandLineArgs()[1];
            string downloadUrl = args.Replace(Constants.REG_NAME + "://", "");
            string fileName = await PrepareFileAsync(downloadUrl);

            PdfDocument doc = new PdfDocument();
            doc.LoadFromFile(fileName);
            doc.Print(new Spire.Pdf.Print.PdfPrintSettings()
            {
                PrinterName = defaultPrinter,
            });
            Application.Exit();

        }

        private static async Task<string> PrepareFileAsync(string downloadUrl)
        {
            string path = Application.StartupPath + Constants.DOWNLOAD_PATH;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            string now = DateTime.Now.ToString("yyyyMMddhhmmss");
            string fileName = path + "\\file_" + now + ".pdf";

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(downloadUrl);
            using (var fs = new FileStream(fileName, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }


            return fileName;
        }

        private void RenderSetting()
        {
            cbPrinters.DataSource = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            string defaultPrinter = ConfigurationManager.AppSettings[Constants.DEFAULT_PRINTER];
            if (defaultPrinter != null)
            {
                cbPrinters.SelectedItem = defaultPrinter;
            }
            
        }

        private static void CheckRegistry()
        {
            bool existed = Registry.ClassesRoot.GetSubKeyNames().Contains(Constants.REG_NAME);
            if (existed) return;

            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated)
            {
                MessageBox.Show("First startup app must be run with administrator privileges", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            RegistryKey key;
            key = Registry.ClassesRoot.CreateSubKey(Constants.REG_NAME);
            key.SetValue("", "URL: Lipps Printer");
            key.SetValue("URL Protocol", "");

            key = key.CreateSubKey("shell");
            key = key.CreateSubKey("open");
            key = key.CreateSubKey("command");
            key.SetValue("", Application.StartupPath + "\\LippsPrinter.exe \"%1\"");

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveCongfig(Constants.DEFAULT_PRINTER, cbPrinters.SelectedValue.ToString());
            Application.Exit();
        }
    }
}