using System;
using System.IO;
using System.Net;
using Ionic.Zip;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace minerstat {
 class Downloader {

  private static string fileName;
  private static string fileNameReal;
  private static int counter;
  private static string downloadUrl = "https://static.minerstat.farm/miners/windows/";
  public static string minerVersion;
  private static string minerType;
  private static string hit;
  private static string decompressStarted;

        internal static bool downloadFile(string v, string n, string cli) {
   bool retVal = false;
   fileName = v;
   fileNameReal = n;
   minerType = cli;
   if (!cli.Equals("cpu")) {
        hit = "0";
   } else
   {
        hit = "1";
   }
   decompressStarted = "false";
   try {
    using(WebClient webClient = new WebClient()) {

     webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
     webClient.DownloadFileAsync(new Uri(downloadUrl + v), v);
     webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DoSomethingOnFinish);

    }

   } catch (Exception value) {
    Program.NewMessage(value.ToString().Substring(0, 42) + "...", "ERROR");
    
   }

   return retVal;
  }


  private static void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {

   bool flag = counter % 100 == 0;
   if (flag) {
   Program.NewMessage("DOWNLOAD => " + fileNameReal.ToUpper() + " (" + e.ProgressPercentage + " %  )", "INFO");
                if (e.ProgressPercentage.Equals(100))
                {
                    if (hit.Equals("0"))
                    {
                        hit = "1";
                        DecompressProtection();                       
                    }
                }
   Program.SyncStatus = false;
   }

  }

        async internal static void DecompressProtection()
        {
           // await Task.Delay(5000);
           // Program.NewMessage("DECOMPRESS => 15s threshold for health check.", "");
            await Task.Delay(15000);
            if (decompressStarted.Equals("false"))
            {
                try
                {
                    Program.NewMessage("DECOMPRESS => Failed. Autofix", "");
                    // KILL ALL MINERS
                    mining.killAll();
                    // STOP TIMERS
                    Program.watchDogs.Stop();
                    Program.syncLoop.Stop();
                    // DELETE BUGGED CLIENTS FOLDER
                    await Task.Delay(2000);
                    Directory.Delete("clients", true);
                    // START MINING
                    await Task.Delay(4000);
                    Program.SyncStatus = false;
                    Program.syncLoop.Stop();
                    await Task.Delay(200);
                    mining.Start();
                }
                catch (Exception) { } 
            } else
            {
          //      Program.NewMessage("DECOMPRESS => Health check: OK!", "");
            }
        }

            async private static void DoSomethingOnFinish(object sender, AsyncCompletedEventArgs e) {

   try {

                File.WriteAllText(Directory.GetCurrentDirectory() + "/clients/" + fileNameReal.ToLower() + "/minerVersion.txt", minerVersion);

                decompressFile();

                File.WriteAllText(Directory.GetCurrentDirectory() + "/clients/" + fileNameReal.ToLower() + "/minerUpdated.txt", minerVersion);

                await Task.Delay(2000);
    mining.downloadConfig(Program.token, Program.worker);
    await Task.Delay(1000);

                Program.NewMessage("NODE => Waiting for the next sync..", "INFO");

                if (minerType.Equals("main"))
                {
                    mining.startMiner(true, false);
                    Program.SyncStatus = true;

                    // Start watchDog
                    Program.watchDogs.Start();

                    // Start SYNC & Remote Command
                    Program.syncLoop.Start();

                } else
                {
                    mining.startMiner(false, true);
                    Program.SyncStatus = true;
                }
                
                

            } catch (Exception) {


            }

  }

  public static void Decompress(string filename, string targetdir) {

   Program.NewMessage("DOWNLOAD => " + fileNameReal.ToUpper() + " ( COMPLETE )", "INFO");
   Program.NewMessage(fileNameReal.ToUpper() + " => Decompressing", "INFO");

            using (ZipFile zipFile = ZipFile.Read(filename))
            {
                zipFile.ExtractProgress +=
               new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
                zipFile.ExtractAll(targetdir, ExtractExistingFileAction.OverwriteSilently);
            }
  }

  private static string decompressFile() {
   Decompress(fileName.ToLower(), Directory.GetCurrentDirectory() + "/clients/" + fileNameReal.ToLower() + "/");
   return "done";
  }

        async static void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.TotalBytesToTransfer > 0)
            {
                Program.NewMessage("UNZIP => " + Convert.ToInt32(100 * e.BytesTransferred / e.TotalBytesToTransfer) + "%", "");
                decompressStarted = "true";

                if (Convert.ToInt32(100 * e.BytesTransferred / e.TotalBytesToTransfer) == 100)
                {
                    try
                    {
                        string safe = fileName.ToLower();
                        await Task.Delay(10000);
                        File.Delete(safe);

                    } catch (Exception ex)
                    {
                        //Program.NewMessage("ERROR" + ex.ToString(), "");
                    }
                }

            }
        }

    }

}