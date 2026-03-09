using System.Diagnostics;
using System.Management;
using TaskManager.App.Models;

namespace TaskManager.App.Services;

public class SystemMonitorService
{
    private PerformanceCounter? cpuCounter;
    private PerformanceCounter? ramCounter;
    private double totalRamGb;

    public SystemMonitorService()
    {
        try
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);

            cpuCounter.NextValue(); 

            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject obj in wmiObject.Get())
            {
                double totalVisibleMemoryKB = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                totalRamGb = totalVisibleMemoryKB / 1024 / 1024; 
                break;
            }
        }
        catch
        {
        }
    }

    public SystemStats GetStats()
    {
        var stats = new SystemStats();

        if (cpuCounter != null && ramCounter != null && totalRamGb > 0)
        {
            try
            {
                stats.CpuUsagePercentage = Math.Round(cpuCounter.NextValue(), 1);
                
                double availableRamMb = ramCounter.NextValue();
                double availableRamGb = availableRamMb / 1024;
                
                stats.TotalRamGB = Math.Round(totalRamGb, 1);
                stats.UsedRamGB = Math.Round(totalRamGb - availableRamGb, 1);
                stats.RamUsagePercentage = Math.Round((stats.UsedRamGB / stats.TotalRamGB) * 100, 1);
            }
            catch
            {
            }
        }

        return stats;
    }
}
