using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace minerstat
{
    class watchDog
    {
        public static Boolean cpuEnabled;
        async public static void health(object sender, ElapsedEventArgs exw)
        {
            try
            {

                if (Process.GetProcessesByName(mining.getProcessName()).Length == 0)
                {

                    if (mining.minerCpu.Equals("False"))
                    {
                        mining.killAll();
                    }

                    Program.NewMessage("WATCHDOG => ERROR", "ERROR");
                    Program.NewMessage("WATCHDOG => " + mining.minerDefault + " is crashed", "ERROR");
                    Program.NewMessage("WATCHDOG => " + mining.minerDefault + " attempt to restart", "INFO");

                    await Task.Delay(1000);

                    if (Program.watchDogFailover >= 5)
                    {
                        Program.NewMessage("FAILOVER => " + mining.minerDefault + " download fresh config.", "INFO");
                        mining.downloadConfig(Program.token, Program.worker);
                        Program.watchDogFailover = 0;
                        mining.startMiner(true, false);
                    }
                    else
                    {
                        mining.startMiner(true, false);
                    }


                    Program.watchDogFailover++;

                }
                else { Program.watchDogFailover = 0; }

            } catch (Exception) { }
        }

        public static void cpuHealth()
        {
            if (mining.minerCpu.Equals("True"))
            {

                //string process = "";

                try
                {
                    

                    if (Process.GetProcessesByName(mining.getCPUProcess()).Length == 0)
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