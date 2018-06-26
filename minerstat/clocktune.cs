using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSI.Afterburner;
using MSI.Afterburner.Exceptions;

namespace minerstat
{
    class clocktune
    {
        public static double memoryBoost, coreBoost, powerLimit, fanSpeed;
        public static string algoType;
        public static ControlMemory macm = new ControlMemory();
        public static HardwareMonitor mahm = new HardwareMonitor();

        public static void Manual(string gpuType, int powerlimit, int coreclock, int fan, int memoryclock)
        {

            for (int i = 0; i < mahm.Header.GpuEntryCount; i++)
            {

                if (!fan.Equals(9999))
                {

                    try
                    {
                        macm.GpuEntries[i].FanSpeedCur = Convert.ToUInt32(fan);
                    }
                    catch (Exception ex)
                    {
                        macm.GpuEntries[i].FanFlagsCur = MACM_SHARED_MEMORY_GPU_ENTRY_FAN_FLAG.None;
                        macm.GpuEntries[i].FanSpeedCur = Convert.ToUInt32(fan);
                    }
                }

                if (!powerlimit.Equals(9999))
                {
                    macm.GpuEntries[i].PowerLimitCur = powerlimit;
                }

                if (gpuType.Equals("nvidia"))
                {
                    if (!coreclock.Equals(9999))
                    {
                        macm.GpuEntries[i].CoreClockBoostCur = coreclock * 1000;
                    }

                    if (!memoryclock.Equals(9999))
                    {
                        macm.GpuEntries[i].MemoryClockBoostCur = memoryclock * 1000;
                    }
                }

                if (gpuType.Equals("amd"))
                {
                    if (!coreclock.Equals(9999))
                    {
                        macm.GpuEntries[i].CoreClockCur = Convert.ToUInt32(coreclock * 1000);
                    }
                    if (!memoryclock.Equals(9999))
                    {
                        macm.GpuEntries[i].MemoryClockCur = Convert.ToUInt32(memoryclock * 1000);

                    }
                }


                // APPLY AFTERBURNER CHANGES
                macm.CommitChanges();
                System.Threading.Thread.Sleep(2000);
                macm.ReloadAll();
            }

        }


        public static void AutoDrive()
        {

            try
            {
     
                algoType = "default";

                switch (mining.minerDefault.ToLower())
                {
                    case "phoenix-eth":
                        algoType = "memory";
                        break;
                    case "claymore-neoscrypt":
                        algoType = "power50";
                        break;
                    case "cast-xmr":
                        algoType = "memory";
                        break;
                    case "stak-xmr":
                        algoType = "memory";
                        break;
                    case "ethminer":
                        algoType = "memory";
                        break;
                    case "claymore-xmr":
                        algoType = "memory";
                        break;
                    case "claymore-zec":
                        algoType = "core";
                        break;
                    case "optiminer-zec":
                        algoType = "core";
                        break;
                    case "sgminer-pasc":
                        algoType = "memory";
                        break;
                    case "ewbf-zec":
                        algoType = "core";
                        break;
                    case "zm-zec":
                        algoType = "core";
                        break;
                }

                for (int i = 0; i < mahm.Header.GpuEntryCount; i++)
                {

                    int memoryMin, memoryMax, coreMin, coreMax, powerMin, powerMax, riseVal;
                    memoryBoost = 0; coreBoost = 0; powerLimit = 100; fanSpeed = 70; riseVal = 0;

                    if (mahm.GpuEntries[i].ToString().Contains("TITAN")) { riseVal = 20; }
                    if (mahm.GpuEntries[i].ToString().Contains("GTX 1080")) { riseVal = 110; }
                    if (mahm.GpuEntries[i].ToString().Contains("GTX 1080 Ti")) { riseVal = 0; }
                    if (mahm.GpuEntries[i].ToString().Contains("GTX 1070")) { riseVal = 40; }
                    if (mahm.GpuEntries[i].ToString().Contains("GTX 1060")) { riseVal = 40; }
                    if (mahm.GpuEntries[i].ToString().Contains("GTX 1050")) { riseVal = 43; }

                    memoryMin = macm.GpuEntries[i].MemoryClockBoostMin + (riseVal * 1000);
                    memoryMax = macm.GpuEntries[i].MemoryClockBoostMax + (riseVal * 1000);
                    coreMin = macm.GpuEntries[i].CoreClockBoostMin + (riseVal * 1000);
                    coreMax = macm.GpuEntries[i].CoreClockBoostMax + (riseVal * 1000);
                    powerMin = macm.GpuEntries[i].PowerLimitMin;
                    powerMax = macm.GpuEntries[i].PowerLimitMax;                   

                    switch (algoType)
                    {
                        case "default":
                            memoryBoost = (memoryMax - (memoryMax * 0.3));
                            coreBoost = (coreMax - (coreMax * 0.3));
                            powerLimit = (powerMin + (powerMax * 0.23));
                            fanSpeed = 75;
                            break;
                        case "core":
                            memoryBoost = (memoryMax - (memoryMax * 0.9));
                            coreBoost = (coreMax - (coreMax * 0.25));
                            powerLimit = (powerMin + (powerMax * 0.20));
                            fanSpeed = 75;
                            break;
                        case "memory":
                            memoryBoost = (memoryMax - (memoryMax * 0.25));
                            coreBoost = (coreMax - (coreMax * 0.8));
                            powerLimit = (powerMin + (powerMax * 0.23));
                            fanSpeed = 75;
                            break;
                        case "power50":
                            memoryBoost = 0;
                            coreBoost = 0;
                            powerLimit = (powerMin + (powerMax * 0));
                            fanSpeed = 80;
                            break;
                    }



                    try
                    {
                        macm.GpuEntries[i].FanSpeedCur = Convert.ToUInt32(fanSpeed);
                    }
                    catch (Exception ex)
                    {
                        macm.GpuEntries[i].FanFlagsCur = MACM_SHARED_MEMORY_GPU_ENTRY_FAN_FLAG.None;
                        macm.GpuEntries[i].FanSpeedCur = Convert.ToUInt32(fanSpeed);
                    }

                    macm.GpuEntries[i].PowerLimitCur = (int)powerLimit;
                    macm.GpuEntries[i].CoreClockBoostCur = (int)Math.Ceiling(coreBoost / 10);
                    macm.GpuEntries[i].MemoryClockBoostCur = (int)Math.Ceiling(memoryBoost / 10);

                }

                // APPLY AFTERBURNER CHANGES
                macm.CommitChanges();
                System.Threading.Thread.Sleep(2000);
                macm.ReloadAll();

            } catch (Exception err)
            {
                Program.NewMessage(err.ToString(), "");
            }

        }


    }
}
