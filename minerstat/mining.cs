using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace minerstat
{
    class mining
    {
        public static string configJSON;
        public static string minerConfig;
        public static string cpuConfig;
        public static string minerDefault;
        public static string cpuDefault;
        public static string minerType;
        public static string minerOverclock;
        public static string minerCpu, remoteVersion;
        private static Form1 _instanceMainForm = null;
        private static string filePath;
        private static string cpuConfigFile;
        private static string cpuVersion;
        private static WebClient wc = new WebClient();
        private static string github_version_file = "https://raw.githubusercontent.com/minerstat/minerstat-windows/master/versionStable.txt";

        public mining(Form1 mainForm)
        {
            _instanceMainForm = mainForm;
        }

        async public static void killAll()
        {
            for (int i = 0; i < 2; i++)
            {
                // STOP TIMERS
                Program.watchDogs.Stop();
                Program.syncLoop.Stop();

                if (Process.GetProcessesByName("powershell").Length > 0)
                {
                    try
                    {
                        System.Diagnostics.Process.Start("taskkill", "/F /IM powershell.exe /T");
                    }
                    catch (Exception)
                    {
                        Program.NewMessage("Unable to close running miner", "ERROR");
                    }
                }

                await Task.Delay(700);

            }

        }

         public static Boolean CheckforUpdates()
        {
                try
                {
                    var localVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    wc.DownloadFile(new Uri(github_version_file), "NetVersion.txt");
                    remoteVersion = File.ReadAllText("NetVersion.txt");
                    File.Delete("NetVersion.txt");

                    if (remoteVersion.Trim() == localVersion.Trim())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception) { return false; }       
        }

        public static void StartUpdate()
        {
            if (!File.Exists("minerstat.exe"))
            {
                MessageBox.Show("Main program file doesn't exist, try reinstalling or update the app.");
                Application.Exit();
            }
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 0 & start minerstat.exe";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }

        async public static void Start()
        {
            minerCpu = "false";
            Program.SyncStatus = false;

            if (Program.StartDelayOver.Equals(false))
            {
                Program.SyncStatus = false;
                Program.NewMessage("INFO => STARTED WITH WINDOWS","INFO");
                Program.NewMessage("INFO => Programmed mining start delay: " + Program.StartDelay + "ms", "INFO");
                await Task.Delay(Program.StartDelay);
                Program.StartDelayOver = true;
                Program.SyncStatus = true;
            }

            if (CheckforUpdates().Equals(true))
            {
                StartUpdate();
                Application.Exit();
            }

            _instanceMainForm.Invoke((MethodInvoker)delegate {
                _instanceMainForm.TopMost = true;
            });

            try
            {

                if (File.Exists(@Program.minerstatDir + "/user.json"))
                {
                    string json = File.ReadAllText(@Program.minerstatDir + "/user.json");
                    Program.loginjson = json;

                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                    Program.token = (string)jObject["token"];
                    Program.worker = (string)jObject["worker"];
                }

                downloadConfig(Program.token, Program.worker);

                if (!Directory.Exists(@Program.currentDir + "/clients"))
                {
                    Directory.CreateDirectory(@Program.currentDir + "/clients");
                }

                modules.getData minersVersion = new modules.getData("https://static.minerstat.farm/miners/windows/version.json", "POST", "");
                string version = minersVersion.GetResponse();

                var vObject = Newtonsoft.Json.Linq.JObject.Parse(version);
                var minerVersion = (string)vObject[minerDefault.ToLower()];
                cpuVersion = (string)vObject[cpuDefault.ToLower()];

                // main MINER

                if (!Directory.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/"))
                {
                    Directory.CreateDirectory(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/");
                }

                string localMinerVersion;

                if (File.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/minerVersion.txt"))
                {
                    localMinerVersion = File.ReadAllText(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/minerVersion.txt");
                }
                else
                {
                    localMinerVersion = "0";
                }

                if (!File.Exists(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/minerUpdated.txt") || !localMinerVersion.Equals(minerVersion))
                {

                    // DELETE ALL FILES
                    System.IO.DirectoryInfo di = new DirectoryInfo(Program.currentDir + "/clients/" + minerDefault.ToLower() + "/");

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    Downloader.minerVersion = minerVersion;

                    // DOWNLOAD FRESH PACKAGE
                    Downloader.downloadFile(minerDefault.ToLower() + ".zip", minerDefault.ToLower(), "main");
                    Program.SyncStatus = false;

                }
                else
                {
                    await Task.Delay(1500);

                    Program.NewMessage("CONFIG => Default miner: " + minerDefault, "INFO");
                    Program.NewMessage("CONFIG => Worker type: " + minerType, "INFO");
                    Program.NewMessage("CONFIG => CPU Mining: " + minerCpu, "INFO");
                    Program.NewMessage(minerDefault.ToUpper() + " => " + minerConfig, "INFO");
                    // Start miner                     
                    Program.NewMessage("NODE => Waiting for the next sync..", "INFO");

                    Program.SyncStatus = true;
                    startMiner(true, false);

                    // Start watchDog
                    Program.watchDogs.Start();

                    // Start Crash Protection
                    //Program.crashLoop.Start();

                    // Start SYNC & Remote Command
                    Program.syncLoop.Start();
                }                      

            }
            catch (Exception ex)
            {
                Program.NewMessage(ex.ToString(), "");
            }

        }

        async public static void startMiner(Boolean m1, Boolean m2)
        {

            if (File.Exists(Program.currentDir + "/" + minerDefault.ToLower() + ".zip"))
            {
                try
                {
                    File.Delete(Program.currentDir + "/" + minerDefault.ToLower() + ".zip");
                } catch (Exception) { Console.WriteLine("ERROR => File .zip removal error"); }
            }


            _instanceMainForm.Invoke((MethodInvoker)delegate {
                _instanceMainForm.TopMost = true;
            });


            if (m1.Equals(true) && m2.Equals(false))
            {
                Program.watchDogs.Stop();
                Program.syncLoop.Stop();
                //Program.crashLoop.Stop();


                if (minerCpu.Equals("True"))
                {

                    if (!Directory.Exists(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/"))
                    {
                        Directory.CreateDirectory(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/");
                    }

                    string cpuMinerVersion;

                    if (File.Exists(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/minerVersion.txt"))
                    {
                        cpuMinerVersion = File.ReadAllText(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/minerVersion.txt");
                    }
                    else
                    {
                        cpuMinerVersion = "0";
                    }

                    if (!File.Exists(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/minerUpdated.txt") || !cpuMinerVersion.Equals(cpuVersion))
                    {

                        // DELETE ALL FILES
                        System.IO.DirectoryInfo di = new DirectoryInfo(Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/");

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        Downloader.minerVersion = cpuVersion;

                        // DOWNLOAD FRESH PACKAGE
                        Program.SyncStatus = false;
                        Downloader.downloadFile(cpuDefault.ToLower() + ".zip", cpuDefault.ToLower(), "cpu");

                    }
                    else
                    {
                        await Task.Delay(1500);
                        startMiner(false, true);
                        Program.SyncStatus = true;
                    }

                }

                Program.watchDogs.Start();
                Program.syncLoop.Start();
                //Program.crashLoop.Start();


            }

            if (m1.Equals(true))
            {
                string folderPath = '"' + Program.currentDir + "/clients/" + minerDefault.ToLower() + "/" + '"';
                Process.Start("C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe ", @"set-location '" + folderPath + "'; " + "./start.bat; pause");              
            }

            if (minerCpu.Equals("True") && m2.Equals(true))
            {
                string folderPath = '"' + Program.currentDir + "/clients/" + cpuDefault.ToLower() + "/" + '"';

                switch (mining.cpuDefault.ToLower())
                {
                    case "xmr-stak-cpu":
                        filePath = "xmr-stak-cpu.exe";
                        break;
                    case "cpuminer-opt":
                        filePath = "start.bat";
                        break;
                    case "xmrig":
                        filePath = "xmrig.exe";
                        break;
                }

                Program.NewMessage(cpuDefault.ToUpper() + " => " + cpuConfig, "INFO");            
                Process.Start("C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe ", @"set-location '" + folderPath + "'; " + "./" + filePath + ";");

            }

            await Task.Delay(2000);

            Program.SyncStatus = true;

            _instanceMainForm.Invoke((MethodInvoker)delegate {
                _instanceMainForm.TopMost = false;
            });

        }

        public static void downloadConfig(string token, string worker)
        {
            try
            {
                modules.getData nodeConfig = new modules.getData("https://api.minerstat.com/v2/node/gpu/" + token + "/" + worker, "POST", "");
                configJSON = nodeConfig.GetResponse();

                var jObject = Newtonsoft.Json.Linq.JObject.Parse(configJSON);
                minerDefault = (string)jObject["default"];
                cpuDefault = (string)jObject["cpuDefault"];
                minerType = (string)jObject["type"];
                minerOverclock = JsonConvert.SerializeObject(jObject["overclock"]);
                minerCpu = (string)jObject["cpu"];
            
                modules.getData configRequest = new modules.getData("https://api.minerstat.com/v2/conf/gpu/" + token + "/" + worker + "/" + minerDefault.ToLower(), "POST", "");
                minerConfig = configRequest.GetResponse();

                string fileExtension = "start.bat";

                if (minerDefault.Contains("claymore"))
                {
                    fileExtension = "config.txt";
                }
                if (minerDefault.Contains("phoenix"))
                {
                    fileExtension = "config.txt";
                }
                if (minerDefault.Contains("sgminer"))
                {
                    fileExtension = "sgminer.conf";
                }
                if (minerDefault.Contains("xmr-stak"))
                {
                    fileExtension = "pools.txt";
                }
                if (minerDefault.Contains("trex"))
                {
                    fileExtension = "config.json";
                }

                string folderPath = Program.currentDir + "/clients/" + minerDefault + "/" + fileExtension;
                File.WriteAllText(@folderPath, minerConfig);

                if (minerCpu.Equals("True"))
                {
                    modules.getData cpuRequest = new modules.getData("https://api.minerstat.com/v2/conf/gpu/" + token + "/" + worker + "/" + cpuDefault.ToLower(), "POST", "");
                    cpuConfig = cpuRequest.GetResponse();

                    switch (cpuDefault.ToLower())
                    {
                        case "xmr-stak-cpu":
                            cpuConfigFile = "config.txt";
                            break;
                        case "cpuminer-opt":
                            cpuConfigFile = "start.bat";
                            break;
                        case "xmrig":
                            cpuConfigFile = "config.json";
                            break;
                    }

                    string folderPathCpu = Program.currentDir + "/clients/"+ cpuDefault.ToLower() + "/" + cpuConfigFile;
                    File.WriteAllText(@folderPathCpu, cpuConfig);
                }

                if (!JsonConvert.SerializeObject(jObject["overclock"]).Equals(""))
                {
                    if (Process.GetProcessesByName("msiafterburner").Length > 0)
                    {
                        try
                        {
                            Program.NewMessage("OVERCLOCK => " + JsonConvert.SerializeObject(jObject["overclock"]), "WARNING");
                        } catch (Exception) { }
                        var mObject = Newtonsoft.Json.Linq.JObject.Parse(JsonConvert.SerializeObject(jObject["overclock"]));
                        var coreclock = (string)mObject["coreclock"];
                        var memoryclock = (string)mObject["memoryclock"];
                        var powerlimit = (string)mObject["powerlimit"];
                        var fan = (string)mObject["fan"];
                        if (coreclock.Equals("skip")) { coreclock = "9999"; }
                        if (memoryclock.Equals("skip")) { memoryclock = "9999"; }
                        if (powerlimit.Equals("skip")) { powerlimit = "9999"; }
                        if (fan.Equals("skip")) { fan = "9999"; }
                        clocktune.Manual(minerType, Convert.ToInt32(powerlimit), Convert.ToInt32(coreclock), Convert.ToInt32(fan), Convert.ToInt32(memoryclock));
                    } else
                    {
                        Program.NewMessage("WARNING => Install MSI Afterburner to enable overclocking" , "WARNING");
                    }

                }

                // Delete pending remote commands
                modules.getData response = new modules.getData("https://api.minerstat.com/v2/get_command_only.php?token=" + Program.token + "&worker=" + Program.worker, "POST", "");
                string responseString = response.GetResponse();

                if (!responseString.Equals(""))
                {
                    Program.NewMessage("PENDING COMMAND REMOVED  => " + responseString, "");
                }

            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

        }

    }
}