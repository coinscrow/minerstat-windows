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
                    try {
                        macm.GpuEntries[i].PowerLimitCur = powerlimit;
                    } catch (Exception powerIssue) { Console.WriteLine(powerIssue.ToString()); }

                }

                if (gpuType.Equals("nvidia"))
                {
                    if (!coreclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[i].CoreClockBoostCur = coreclock * 1000;
                        }
                        catch (Exception coreIssue) { Program.NewMessage(coreIssue.ToString(), ""); }
                    }

                    if (!memoryclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[i].MemoryClockBoostCur = memoryclock * 1000;
                        }
                        catch (Exception memoryIssue) { Console.WriteLine(memoryIssue.ToString()); }
                    }
                }

                if (gpuType.Equals("amd"))
                {
                    if (!coreclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[i].CoreClockCur = Convert.ToUInt32(coreclock * 1000);
                        }
                        catch (Exception coreIssue) { Program.NewMessage(coreIssue.ToString(), ""); }
                    }
                    if (!memoryclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[i].MemoryClockCur = Convert.ToUInt32(memoryclock * 1000);
                        }
                        catch (Exception memoryIssue) { Console.WriteLine(memoryIssue.ToString()); }
                    }
                }

                try
                {
                    // APPLY AFTERBURNER CHANGES
                    macm.CommitChanges();
                    System.Threading.Thread.Sleep(2000);
                    macm.ReloadAll();
                } catch (Exception applySettings)
                {
                   // Program.NewMessage("" + applySettings.ToString(), "");
                }

            }

        }


        public static void Advanced(string gpuType, int powerlimit, int coreclock, int fan, int memoryclock, int gpuid)
        {
    
                if (!fan.Equals(9999))
                {

                    try
                    {
                        macm.GpuEntries[gpuid].FanSpeedCur = Convert.ToUInt32(fan);
                    }
                    catch (Exception ex)
                    {
                        macm.GpuEntries[gpuid].FanFlagsCur = MACM_SHARED_MEMORY_GPU_ENTRY_FAN_FLAG.None;
                        macm.GpuEntries[gpuid].FanSpeedCur = Convert.ToUInt32(fan);
                    }
                }

                if (!powerlimit.Equals(9999))
                {
                    try
                    {
                        macm.GpuEntries[gpuid].PowerLimitCur = powerlimit;
                    }
                    catch (Exception powerIssue) { Console.WriteLine(powerIssue.ToString()); }

                }

                if (gpuType.Equals("nvidia"))
                {
                    if (!coreclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[gpuid].CoreClockBoostCur = coreclock * 1000;
                        }
                        catch (Exception coreIssue) { Program.NewMessage(coreIssue.ToString(), ""); }
                    }

                    if (!memoryclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[gpuid].MemoryClockBoostCur = memoryclock * 1000;
                        }
                        catch (Exception memoryIssue) { Console.WriteLine(memoryIssue.ToString()); }
                    }
                }

                if (gpuType.Equals("amd"))
                {
                    if (!coreclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[gpuid].CoreClockCur = Convert.ToUInt32(coreclock * 1000);
                        }
                        catch (Exception coreIssue) { Program.NewMessage(coreIssue.ToString(), ""); }
                    }
                    if (!memoryclock.Equals(9999))
                    {
                        try
                        {
                            macm.GpuEntries[gpuid].MemoryClockCur = Convert.ToUInt32(memoryclock * 1000);
                        }
                        catch (Exception memoryIssue) { Console.WriteLine(memoryIssue.ToString()); }
                    }
                }

                try
                {
                    // APPLY AFTERBURNER CHANGES
                    macm.CommitChanges();
                    System.Threading.Thread.Sleep(2000);
                    macm.ReloadAll();
                }
                catch (Exception)
                {
                    // Program.NewMessage("" + applySettings.ToString(), "");
                }

        }

    }
}
