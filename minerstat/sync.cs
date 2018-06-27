using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Http;
using System.Windows.Forms;
using System.Diagnostics;


namespace minerstat
{
    class sync
    {

        public static string monitorURL;
        public static string apiResponse;
        public static string apiHardware;
        public static string apiCpu;
        private static readonly HttpClient client = new HttpClient();
        public static PerformanceCounter ramCounter;


        async public static void loop(object sender, ElapsedEventArgs exw)
        {

            try
            {         
            
                if (modules.checkNet(false) == false)
                {
                    Program.NewMessage("SYNC => Skip: CONNECTION LOST", "ERROR");
                }
                else
                {

                    if (modules.IsReach().Equals(false))
                    {
                        Program.NewMessage("SYNC => Skip: MINERSTAT UNREACHABLE", "ERROR");
                    }
                    else
                    {

                        // SET null
                        apiResponse = "";
                        apiHardware = "";
                        apiCpu = "";


                        // 1) PREPARE THE URL'S if Needed
                        switch (mining.minerDefault.ToLower())
                        {
                            case "cast-xmr":
                                monitorURL = "http://127.0.0.1:7777";
                                break;
                            case "xmr-stak":
                                monitorURL = "http://127.0.0.1:2222/api.json";
                                break;
                            case "bminer":
                                monitorURL = "http://127.0.0.1:1880/api/status";
                                break;
                        }


                        // 2) Fetch API's
                        if (mining.minerDefault.ToLower().Contains("ccminer")) { modules.getStat(); }
                        if (mining.minerDefault.ToLower().Contains("ewbf")) { modules.getStat_ewbf(); }
                        if (mining.minerDefault.ToLower().Contains("zm-zec")) { modules.getStat_zm(); }
                        if (mining.minerDefault.ToLower().Contains("phoenix-eth") || mining.minerDefault.ToLower().Contains("claymore")) { modules.getStat_claymore(); }
                        if (mining.minerDefault.ToLower().Contains("sgminer")) { modules.getStat_sgminer(); }
                        if (mining.minerDefault.ToLower().Contains("gateless")) { modules.getStat_sgminer(); }
                        if (mining.minerDefault.ToLower().Contains("ethminer")) { apiResponse = "skip"; }
                        if (mining.minerDefault.ToLower().Contains("cast-xmr") || mining.minerDefault.ToLower().Contains("xmr-stak") || mining.minerDefault.ToLower().Contains("bminer"))
                        {

                            string input;
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(monitorURL);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            StreamReader sr = new StreamReader(response.GetResponseStream());
                            input = sr.ReadToEnd();
                            sr.Close();
                            apiResponse = input;

                        }

                        // Hardware Monitor
                        modules.getData hwQuery = new modules.getData("http://localhost:" + Program.monitorport + "/", "POST", "");
                        apiHardware = hwQuery.GetResponse();

                        // CPU Miner's
                        if (mining.minerCpu.Equals("True"))
                        {
                            switch (mining.cpuDefault.ToLower())
                            {
                                case "xmr-stak-cpu":
                                    monitorURL = "HTTP";
                                    break;
                                case "cpuminer-opt":
                                    monitorURL = "TCP";
                                    break;
                                case "xmrig":
                                    monitorURL = "HTTP";
                                    break;
                            }

                            try
                            {
                                if (monitorURL.Equals("HTTP"))
                                {

                                    string input;
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:7887");
                                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                    StreamReader sr = new StreamReader(response.GetResponseStream());
                                    input = sr.ReadToEnd();
                                    sr.Close();
                                    apiCpu = input;

                                }
                                else
                                {
                                    modules.getStat_cpu();
                                }
                            } catch (Exception ex)
                            {
                                Program.NewMessage("ERROR => CPU API NOT RUNNING", "");
                                watchDog.cpuHealth();
                            }

                        }

                        // 4) POST

                        if (!mining.minerDefault.ToLower().Contains("ethminer"))
                        {
                            await Task.Delay(1000);

                            try
                            {

                                var postValue = new Dictionary<string, string>
                        {
                        { "minerData", apiResponse },
                        { "hwData", apiHardware },
                        { "cpuData", apiCpu }
                        };

                                var content = new FormUrlEncodedContent(postValue);
                                try
                                {
                                    ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
                                }
                                catch (Exception ram) { }

                                var response = await client.PostAsync("https://api.minerstat.com/v2/set_node_config.php?token=" + Program.token + "&worker=" + Program.worker + "&miner=" + mining.minerDefault.ToLower() + "&ver=4&cpuu=" + mining.minerCpu + "&cpud=HASH" + "&os=win" + "&algo=&best=&space=" + modules.GetTotalFreeSpace("C") / 1000000 + "&freemem=" + Convert.ToInt32(ramCounter.NextValue()).ToString() + "&localip=" + modules.GetLocalIPAddress() + "&remoteip=" + modules.GetUserIP() + "&currentcpu=" + mining.cpuDefault.ToLower(), content);
                                var responseString = await response.Content.ReadAsStringAsync();

                                if (!responseString.Equals(""))
                                {
                                    Program.NewMessage("REMOTE COMMAND => " + responseString, "");
                                    RemoteCommand(responseString);
                                }

                              
                                int package = (apiHardware.Length + apiResponse.Length + apiCpu.Length) * sizeof(Char);
                                modules.updateTraffic(package);

                                Program.NewMessage("SYNC => API Updated [ ~ " + (package / 1000) + " KB ]", "INFO");


                            }
                            catch (Exception ex) { }
                        } else
                        {

                            // Only Remote Command Check
                            modules.getData response = new modules.getData("https://api.minerstat.com/v2/get_command_only.php?token=" + Program.token + "&worker=" + Program.worker, "POST", "");
                            string responseString = response.GetResponse();

                            if (!responseString.Equals(""))
                            {
                                Program.NewMessage("REMOTE COMMAND => " + responseString, "");
                                RemoteCommand(responseString);
                            }

                        }


                    }


                }

            }
            catch (Exception error)
            {
                Program.NewMessage(error.ToString(), "");
            }


        }

        async public static void RemoteCommand(string command)
        {

            if (command.Equals("RESTARTNODE"))
            {
                mining.killAll();
                Program.watchDogs.Stop();
                Program.syncLoop.Stop();
                await Task.Delay(1000);
                mining.Start();
            }

            if (command.Equals("REBOOT"))
            {
                mining.killAll();
                Program.watchDogs.Stop();
                Program.syncLoop.Stop();
                await Task.Delay(1000);
                System.Diagnostics.Process.Start("shutdown.exe", "-r -f -t 0");
            }

            if (command.Equals("SHUTDOWN"))
            {
                mining.killAll();
                Program.watchDogs.Stop();
                Program.syncLoop.Stop();
                await Task.Delay(1000);
                System.Diagnostics.Process.Start("shutdown.exe", "-s -f -t 0");
            }

            if (command.Equals("DOWNLOADWATTS"))
            {
                mining.downloadConfig(Program.token, Program.worker);
            }

            if (command.Equals("RESTARTWATTS"))
            {
                mining.killAll();
                Program.watchDogs.Stop();
                Program.syncLoop.Stop();
                await Task.Delay(1500);
                mining.downloadConfig(Program.token, Program.worker);
                await Task.Delay(1000);
                mining.Start();
            }


        }

    }
}
