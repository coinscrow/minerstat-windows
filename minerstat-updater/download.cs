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

        private static string fileName;
        private static string fileNameReal;
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
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
                    webClient.DownloadFileAsync(new Uri("https://ci.appveyor.com/api/projects/minerstat/minerstat-asic/artifacts/release-builds/minerstat-asic-windows.zip"), "update.zip");
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
                _instanceMainForm.progressBar.Invoke((MethodInvoker)delegate {
                    _instanceMainForm.progressBar.Value = e.ProgressPercentage;
                });

            }

        }


        private static void DoSomethingOnFinish(object sender, AsyncCompletedEventArgs e)
        {

            try
            {

                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/tmp/"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/tmp/");
                }

                Decompress("update.zip", "/tmp");


            }
            catch (Exception)
            {


            }

        }

        public static void Decompress(string filename, string targetdir)
        {

            using (ZipFile zipFile = ZipFile.Read(filename))
            {
                zipFile.ExtractProgress +=
               new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
                zipFile.ExtractAll(targetdir, ExtractExistingFileAction.OverwriteSilently);
            }

        }


        async static void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.TotalBytesToTransfer > 0)
            {

                _instanceMainForm.progressBar.Invoke((MethodInvoker)delegate {
                    _instanceMainForm.progressBar.Value = Convert.ToInt32(100 * e.BytesTransferred / e.TotalBytesToTransfer);
                });

                if (Convert.ToInt32(100 * e.BytesTransferred / e.TotalBytesToTransfer) == 100)
                {
                    try
                    {
                        string safe = fileName.ToLower();

                        if (dl.Equals(false))
                        {
                            dl = true;
                            LauncherForm.doTask();
                            // SAVE
                            File.WriteAllText(@Program.minerstatDir + "/version.txt", minerVersion);
                        }

                        await Task.Delay(10000);
                        File.Delete(safe);                      

                    }
                    catch (Exception ex)
                    {
                        //Program.NewMessage("ERROR" + ex.ToString(), "");
                    }
                }

            }
        }

    }

}