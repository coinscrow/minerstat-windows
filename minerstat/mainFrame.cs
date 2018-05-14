using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private static Form1 _instanceMainForm = null;
        public Form1 form1 = null;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
       

        public void dragMe()
        {
            try
            {
                _instanceMainForm.Invoke((MethodInvoker)delegate
                {
                    ReleaseCapture();
                    SendMessage(_instanceMainForm.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

                });
            } catch (Exception ex) { }

        }

        public mainFrame(ChromiumWebBrowser originalBrowser, Form1 mainForm)
        {
            _instanceBrowser = originalBrowser;
            _instanceMainForm = mainForm;
        }

        public void showDevTools()
        {
            _instanceBrowser.ShowDevTools();
        }

        public void closeApp()
        {
            mining.killAll();
            // STOP TIMERS
            Program.watchDogs.Stop();
            Program.syncLoop.Stop();

            System.Windows.Forms.Application.Exit();
        }

        public void minApp()
        {
            _instanceMainForm.Invoke((MethodInvoker)delegate { _instanceMainForm.WindowState = FormWindowState.Minimized; });
        }

        public void openURL(string URL)
        {
            System.Diagnostics.Process.Start(URL);
        }

        public void logOut()
        {
            if (File.Exists(Program.minerstatDir + "/user.json"))
            {
                File.Delete((Program.minerstatDir + "/user.json"));
            }

            mining.killAll();
            // STOP TIMERS
            Program.watchDogs.Stop();
            Program.syncLoop.Stop();

            Application.Restart();

        }

        public void restartApp()
        {

            // STOP TIMERS
            Program.watchDogs.Stop();
            Program.syncLoop.Stop();

            // AUTO UPDATE IF AVAILABLE

            Application.Restart();

        }

        public void miningStop()
        {

        Program.NewMessage("USER => Mining stop", "INFO");
            // STOP TIMERS
            Program.watchDogs.Stop();
            Program.syncLoop.Stop();

            mining.killAll();

        }

        public void miningStart()
        {

        Program.NewMessage("USER => Mining start", "INFO");

        mining.killAll();
        mining.Start(); 

        }


        public Boolean netCheck()
        {
            Boolean response = false;

            response = modules.checkNet(true);

            return response;
        }

        public void newMessage(string text, string type)
        {
            Program.NewMessage(text, type);
        }

        public string getDisplay()
        {

            var response = "";

            lock (((ICollection)minerstat.Program.Message).SyncRoot)
            {
                foreach (string msg in Program.Message)
                {
                    response += msg;
                }
            }

            return response;

        }

        public string getUser()
        {
            var response = Program.loginjson;
            return response;
        }

        public string getWorker()
        {
            var response = Program.worker;
            return response;
        }


        public string getIP()
        {
            var response = modules.GetLocalIPAddress();
            return response;
        }

        public string getTraffic()
        {
            var response = Program.totalTraffic + "&nbsp;" + Program.suffix;
            return response;
        }


        public class loginUser
        {
            public string token { get; set; }
            public string worker { get; set; }
        }

        public void setUser(string Gtoken, string Gworker)
        {


            if (!Directory.Exists(Program.minerstatDir))
            {
                Directory.CreateDirectory(Program.minerstatDir);
            }

            if (File.Exists(Program.minerstatDir + "/user.json"))
            {
                File.Delete((Program.minerstatDir + "/user.json"));
            }

            loginUser loginUser = new loginUser
            {
                token = Gtoken,
                worker = Gworker
            };


            File.WriteAllText(@Program.minerstatDir + "/user.json", JsonConvert.SerializeObject(loginUser));


        }



    }

}