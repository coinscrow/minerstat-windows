using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows.Forms;

namespace minerstat
{
    class watchDog
    {
        public static string process;
        public static Boolean cpuEnabled;
        public static void health(object sender, ElapsedEventArgs exw)
        {     
            switch (mining.minerDefault.ToLower())
            {
                case "phoenix-eth":
                    process = "phoenixminer";
                    break;
                case "claymore-neoscrypt":
                    process = "neoscryptminer";
                    break;
                case "ccminer-tpruvot":
                    process = "ccminer-80-x64";
                    break;
                case "cast-xmr":
                    process = "cast_xmr-vega";
                    break;
                case "xmr-stak":
                    process = "xmr-stak";
                    break;
                case "ccminer-alexis":
                    process = "ccmineralexis78";
                    break;
                case "ccminer-x16r":
                    process = "ccminer";
                    break;
                case "bminer":
                    process = "bminer";
                    break;
                case "ccminer-krnlx":
                    process = "ccminer";
                    break;
                case "ethminer":
                    process = "ethminer";
                    break;
                case "claymore-xmr":
                    process = "nsgpucnminer";
                    break;
                case "claymore-eth":
                    process = "ethdcrminer64";
                    break;
                case "claymore-zec":
                    process = "zecminer64";
                    break;
                case "optiminer-zec":
                    process = "optiminer";
                    break;
                case "sgminer-pasc":
                    process = "sgminer";
                    break;
                case "gatelessgate":
                    process = "gatelessgate";
                    break;
                case "sgminer-gm":
                    process = "sgminer";
                    break;
                case "ewbf-zec":
                    process = "miner";
                    break;
                case "ewbf-zhash":
                    process = "miner";
                    break;
                case "zm-zec":
                    process = "zm";
                    break;
            }

            if (Process.GetProcessesByName(process).Length == 0)
            {

                Program.NewMessage("WATCHDOG => ERROR", "ERROR");
                Program.NewMessage("WATCHDOG => " + mining.minerDefault + " is crashed", "ERROR");
                Program.NewMessage("WATCHDOG => " + mining.minerDefault + " attempt to restart", "INFO");


                if (Program.watchDogFailover >= 5)
                {
                    Program.NewMessage("FAILOVER => " + mining.minerDefault + " download fresh config.", "INFO");
                    mining.downloadConfig(Program.token, Program.worker);
                    Program.watchDogFailover = 0;
                    mining.startMiner(true, false);
                } else
                {
                    mining.startMiner(true, false);
                }


                Program.watchDogFailover ++;

            } else { Program.watchDogFailover = 0; }

        }

        public static void cpuHealth()
        {
            if (mining.minerCpu.Equals("True"))
            {

                try
                {
                    switch (mining.cpuDefault.ToLower())
                    {
                        case "xmr-stak-cpu":
                            process = "xmr-stak-cpu";
                            break;
                        case "cpuminer-opt":
                            process = "cpuminer-celeron";
                            break;
                        case "xmrig":
                            process = "xmrig";
                            break;
                    }

                    if (Process.GetProcessesByName(process).Length == 0)
                    {
                        Program.NewMessage("WATCHDOG => ERROR", "ERROR");
                        Program.NewMessage("WATCHDOG => " + mining.cpuDefault + " is crashed", "ERROR");
                        Program.NewMessage("WATCHDOG => " + mining.cpuDefault + " attempt to restart", "INFO");

                        if (Program.watchDogFailoverCpu >= 5)
                        {
                            Program.NewMessage("FAILOVER => " + mining.cpuDefault + " download fresh config.", "INFO");
                            mining.downloadConfig(Program.token, Program.worker);
                            Program.watchDogFailoverCpu = 0;
                            mining.startMiner(false, true);
                        }
                        else
                        {
                            mining.startMiner(false, true);
                        }

                        Program.watchDogFailover++;

                    } else { Program.watchDogFailoverCpu = 0; }
                }
                catch (Exception) { }
            }
        }


        }
    }