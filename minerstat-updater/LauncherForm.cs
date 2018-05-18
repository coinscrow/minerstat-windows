using System;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using CefSharp;
using System.Drawing;
using CefSharp.WinForms;
using System.Threading.Tasks;

namespace Launcher
{
    public partial class LauncherForm : Form
    {
        private WebClient wc = new WebClient();  
        private string name_of_program = "daemon.exe";
        private string github_version_file = "https://raw.githubusercontent.com/minerstat/minerstat-windows/master/version.txt";
        public ChromiumWebBrowser chromeBrowser;
        public static string remoteVersion;
        DropShadow ds = new DropShadow();

        public LauncherForm()
        {
            InitializeComponent();
            InitializeChromium();
            chromeBrowser.RegisterJsObject("delegate", new Downloader(this));
            this.Resize += new EventHandler(Form1_Resize);
            this.LocationChanged += new EventHandler(Form1_Resize);
        }

        void Form1_Shown()
        {
            Rectangle rc = this.Bounds;
            rc.Inflate(2, 2);
            ds.Bounds = rc;
            ds.Show();
            this.BringToFront();
        }
        void Form1_Resize(object sender, EventArgs e)
        {
            ds.Visible = (this.WindowState == FormWindowState.Normal);
            if (ds.Visible)
            {
                Rectangle rc = this.Bounds;
                rc.Inflate(2, 2);
                ds.Bounds = rc;
            }
            this.BringToFront();
        }

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();

            // Note that if you get an error or a white screen, you may be doing something wrong !
            // Try to load a local file that you're sure that exists and give the complete path instead to test
            // for example, replace page with a direct path instead :
            // String page = @"C:\Users\SDkCarlos\Desktop\afolder\index.html";

            String page = string.Format(@"{0}\asset\index.html", Application.StartupPath);
            //String page = @"C:\Users\SDkCarlos\Desktop\artyom-HOMEPAGE\index.html";

            if (!File.Exists(page))
            {
                MessageBox.Show("Error The html file doesn't exists : " + page);
            }

            // Initialize cef with the provided settings
            //Cef.Initialize(settings);
            // Create a browser component
            chromeBrowser = new ChromiumWebBrowser(page);

            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;

            // Allow the use of local resources in the browser
            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            chromeBrowser.BrowserSettings = browserSettings;
            Form1_Shown();
        }

        private void LauncherForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(@Program.minerstatDir + "/version.txt"))
            {
               
                try
                {
                    var localVersion = File.ReadAllText(@Program.minerstatDir + "/version.txt");
                    var remoteVersionQuery = new Program.getData(github_version_file, "POST", "");
                    remoteVersion = remoteVersionQuery.GetResponse();

                    if (!localVersion.Equals(remoteVersion))
                    {
                        Downloader.minerVersion = remoteVersion;
                        Downloader.dl = false;
                        Downloader.downloadFile();
                    } else
                    {
                        StartApp();
                    }
                } catch (Exception ex) { Application.Restart(); }

            } else
            {
                Downloader.minerVersion = remoteVersion;
                Downloader.dl = false;
                Downloader.downloadFile();
            }
        }
   
 
        private bool LookForProgram()
        {
            string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
            foreach (string filePath in filePaths)
            {
                var name = new FileInfo(filePath).Name;
                if (name == name_of_program)
                    return true;
            }
            return false;
        }

        private void StartApp()
        {
            if (!File.Exists(name_of_program))
            {
                MessageBox.Show("Main program file doesn't exist, try reinstalling or updating app.");
                Application.Exit();
            }
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 0 & start " + name_of_program + " --verify e3546rfgre3t";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }

        public static void StartAppStatic()
        {
            if (!File.Exists("daemon.exe"))
            {
                MessageBox.Show("Main program file doesn't exist, try reinstalling or updating app.");
                Application.Exit();
            }
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 0 & start " + "daemon.exe" + " --verify e3546rfgre3t";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }


        async public static void doTask()
        {
            try {
                await Task.Delay(1000);
                if (File.Exists(@Program.currentDir + "/daemon.exe"))
                {
                    File.Delete(@Program.currentDir + "/daemon.exe");
                }
                await Task.Delay(2000);
                // File.Move -> After Special char Windows testing
                File.Copy(Program.currentDir + "/daemon2.exe", @Program.currentDir + "/daemon.exe");
                await Task.Delay(2000);
                if (File.Exists(@Program.currentDir + "/daemon2.exe"))
                {
                    File.Delete(@Program.currentDir + "/daemon2.exe");
                }
                await Task.Delay(1000);
                StartAppStatic();
            } catch (Exception ex) { File.WriteAllText("file.txt",ex.ToString());  //Application.Restart(); 
            }
           
        }


  }

    public class DropShadow : Form
    {
        public DropShadow()
        {
            this.Opacity = 0.1;
            this.BackColor = Color.Gray;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
        }
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_NOACTIVATE = 0x8000000;
        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
                return cp;
            }
        }
    }

}

