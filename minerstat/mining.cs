using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public static string minerCpu;
        private static Form1 _instanceMainForm = null;
        private static string filePath;
        private static string cpuConfigFile;
        private static string cpuVersion;

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

        async public static void Start()
        {

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

                Program.NewMessage("CONFIG => Default miner: " + minerDefault, "INFO");
                Program.NewMessage("CONFIG => Worker type: " + minerType, "INFO");
                Program.NewMessage("CONFIG => CPU Mining: " + minerCpu, "INFO");
                Program.NewMessage(minerDefault.ToUpper() + " => " + minerConfig, "INFO");

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

                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/clients/" + minerDefault.ToLower() + "/"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/clients/" + minerDefault.ToLower() + "/");
                }

                string localMinerVersion;

                if (File.Exists(Directory.GetCurrentDirectory() + "/clients/" + minerDefault.ToLower() + "/minerVersion.txt"))
                {
                    localMinerVersion = File.ReadAllText(Directory.GetCurrentDirectory() + "/clients/" + minerDefault.ToLower() + "/minerVersion.txt");
                }
                else
                {
                    localMinerVersion = "0";
                }

                if (!File.Exists(Directory.GetCurrentDirectory() + "/clients/" + minerDefault.ToLower() + "/minerUpdated.txt") || !localMinerVersion.Equals(minerVersion))
                {

                    // DELETE ALL FILES
                    System.IO.DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory() + "/clients/" + minerDefault.ToLower() + "/");

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
                    // Start miner                     
                    Program.NewMessage("NODE => Waiting for the first sync..", "INFO");

                    Program.SyncStatus = true;
                    startMiner(true, false);

                    // Start watchDog
                    Program.watchDogs.Start();

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

            if (m1.Equals(true) && m2.Equals(false))
            {

                if (minerCpu.Equals("True"))
                {

                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/clients/" + cpuDefault.ToLower() + "/"))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/clients/" + cpuDefault.ToLower() + "/");
                    }

                    string cpuMinerVersion;

                    if (File.Exists(Directory.GetCurrentDirectory() + "/clients/" + cpuDefault.ToLower() + "/minerVersion.txt"))
                    {
                        cpuMinerVersion = File.ReadAllText(Directory.GetCurrentDirectory() + "/clients/" + cpuDefault.ToLower() + "/minerVersion.txt");
                    }
                    else
                    {
                        cpuMinerVersion = "0";
                    }

                    if (!File.Exists(Directory.GetCurrentDirectory() + "/clients/" + cpuDefault.ToLower() + "/minerUpdated.txt") || !cpuMinerVersion.Equals(cpuVersion))
                    {

                        // DELETE ALL FILES
                        System.IO.DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory() + "/clients/" + cpuDefault.ToLower() + "/");

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

            }


            if (m1.Equals(true))
            {
                string folderPath = Directory.GetCurrentDirectory() + "/clients/" + minerDefault.ToLower() + "/";
                System.Diagnostics.Process.Start("C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe ", "cd " + folderPath + "; " + folderPath + "/start.bat");
            }

            if (minerCpu.Equals("True") && m2.Equals(true))
            {
                string folderPath = Directory.GetCurrentDirectory() + "/clients/" + cpuDefault.ToLower() + "/";

                switch (mining.cpuDefault.ToLower())
                {
                    case "xmr-stak-cpu":
                        filePath = "xmr-stak-cpu.exe";
                        break;
                    case "cpuminer-opt":
                        filePath = "start.bat" +
                            "";
                        break;
                    case "xmrig":
                        filePath = "xmrig.exe";
                        break;
                }

                Program.NewMessage(cpuDefault.ToUpper() + " => " + cpuConfig, "INFO");            
                    System.Diagnostics.Process.Start("C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe ", "cd " + folderPath + "; " + folderPath + "/" + filePath);
       
            }

            await Task.Delay(2000);

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
                minerOverclock = (string)jObject["overclock"];
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

                string folderPath = Directory.GetCurrentDirectory() + "/clients/" + minerDefault + "/" + fileExtension;
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

                    string folderPathCpu = Directory.GetCurrentDirectory() + "/clients/"+ cpuDefault.ToLower() + "/" + cpuConfigFile;
                    File.WriteAllText(@folderPathCpu, cpuConfig);
                }

                if (!minerOverclock.Equals(""))
                {
                    if (Process.GetProcessesByName("msiafterburner").Length > 0)
                    {
                        var mObject = Newtonsoft.Json.Linq.JObject.Parse(minerOverclock);
                        clocktune.Manual(minerType, (int)mObject["powerlimit"], (int)mObject["coreclock"], (int)mObject["fan"], (int)mObject["memoryclock"]);
                    } else
                    {
                        Program.NewMessage("WARNING > Install MSI Afterburner to enable overclocking" , "WARNING");
                    }

                }

            }
            catch (Exception ex) { }

        }

    }
}