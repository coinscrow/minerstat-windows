using System;
using System.IO;
using System.Net;
using Ionic.Zip;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    class Downloader
    {

        public static string fileName;
        private static int counter;
        public static string minerVersion;
        private static LauncherForm _instanceMainForm = null;
        public static Boolean dl;

        public Downloader(LauncherForm mainForm)
        {
            _instanceMainForm = mainForm;
        }

        internal static bool downloadFile()
        {
            bool retVal = false;
            fileName = "update.zip";
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://github.com/minerstat/minerstat-windows/releases/download/latest/minerstat-portable.zip");
                request.AllowAutoRedirect = false;
                request.UserAgent = "minerstat v1.0 (Node)";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string redirUrl = response.Headers["Location"];
                response.Close();

               // MessageBox.Show(redirUrl);

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
                    webClient.Headers.Add("user-agent", "minerstat v1.0 (Node)");
                    webClient.DownloadFileAsync(new Uri(redirUrl), "update.zip");
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DoSomethingOnFinish);

                }

            }
            catch (Exception value)
            {
                MessageBox.Show(value.ToString());
            }

            return retVal;
        }


        private static void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

            bool flag = counter % 100 == 0;
            if (flag)
            {
                //MessageBox.Show(Convert.ToInt32(e.ProgressPercentage).ToString());
                
                    minerstat.mainFrame.progressValue = e.ProgressPercentage;
          

            }

        }


        async private static void DoSomethingOnFinish(object sender, AsyncCompletedEventArgs e)
        {

            try
            {

                if (!Directory.Exists(Program.currentDir + "/update/"))
                {
                    Directory.CreateDirectory(Program.currentDir + "/update");
                }

                System.IO.DirectoryInfo di = new DirectoryInfo(Program.currentDir + "/");

                try
                {
                    File.Delete("daemon.exe");
                    Directory.Delete("asset", true);
                } catch (Exception) {  }
                using (ZipFile zipFile = ZipFile.Read("update.zip"))
                {

                    foreach (ZipEntry fileName in zipFile)
                    {
                        try
                        {
                            fileName.Extract(Program.currentDir + "/", ExtractExistingFileAction.DoNotOverwrite);
                        } catch (Exception) {  }
                    }

                    try
                    {
                        await Task.Delay(7000);
                        LauncherForm.doTask();
                        await Task.Delay(2000);
                    } catch (Exception) {  }

                }


            }
            catch (Exception)
            {
                
            }

        }

        protected static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

    }

}