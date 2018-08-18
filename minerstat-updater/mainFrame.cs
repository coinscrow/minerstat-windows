using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;

namespace minerstat
{

    class mainFrame
    {

        // Declare a local instance of chromium and the main form in order to execute things from here in the main thread
        private static ChromiumWebBrowser _instanceBrowser = null;
        // The form class needs to be changed according to yours
        private static Launcher.LauncherForm _instanceMainForm = null;
        public Launcher.LauncherForm form1 = null;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public static int progressValue;


        public void dragMe()
        {
            try
            {
                _instanceMainForm.Invoke((MethodInvoker)delegate
                {
                    ReleaseCapture();
                    SendMessage(_instanceMainForm.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

                });
            }
            catch (Exception) { }

        }

        public mainFrame(ChromiumWebBrowser originalBrowser, Launcher.LauncherForm mainForm)
        {
            _instanceBrowser = originalBrowser;
            _instanceMainForm = mainForm;
        }

        public void showDevTools()
        {
            _instanceBrowser.ShowDevTools();
        }


        public void loaded()
        {
            Launcher.LauncherForm.Loaded();
        }

        public void minApp()
        {
            _instanceMainForm.Invoke((MethodInvoker)delegate { _instanceMainForm.WindowState = FormWindowState.Minimized; });
        }

        public void openURL(string URL)
        {
            System.Diagnostics.Process.Start(URL);
        }

        public int getProgress()
        {
            return progressValue;
        }

       
        public Boolean netCheck()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "minerstat.farm";
                byte[] buffer = new byte[32];
                int timeout = 2000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {                   
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


    }

}