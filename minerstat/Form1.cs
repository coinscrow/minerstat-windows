using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;


namespace minerstat
{
    public partial class Form1 : Form
    {

        public ChromiumWebBrowser chromeBrowser;

       
        public Form1()
        {
            
            InitializeComponent();

            if (File.Exists(@Program.minerstatDir + "/user.json"))
            {
                string json = File.ReadAllText(@Program.minerstatDir + "/user.json");
                Program.loginjson = json;

                var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                Program.token = (string)jObject["token"];
                Program.worker = (string)jObject["worker"];
            }

            InitializeChromium();
            chromeBrowser.RegisterJsObject("doFrame", new mainFrame(chromeBrowser, this));
            chromeBrowser.RegisterJsObject("mining", new mining(this));

            bunifuDragControl1.TargetControl = chromeBrowser;
        }


        private void frameLoad(object sender, EventArgs e)
        {
            hardwareMonitor.jsonserver();
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
            Cef.Initialize(settings);
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
        }


        private void frameClosing(object sender, FormClosingEventArgs e)
        {
            //  Cef.Shutdown();
        }


        private void frameClick(object sender, MouseEventArgs e)
        {

        }





    }
}
