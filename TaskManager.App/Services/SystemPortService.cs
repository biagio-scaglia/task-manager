using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskManager.App.Models;

namespace TaskManager.App.Services;

public class SystemPortService
{
    public async Task<List<SystemPort>> GetOpenPortsAsync()
    {
        var ports = new List<SystemPort>();
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "netstat.exe",
            Arguments = "-ano",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(processStartInfo);
            if (process != null)
            {
                using var reader = process.StandardOutput;
                string line = await reader.ReadToEndAsync();
                
                var regex = new Regex(@"^\s+(TCP|UDP)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([0-9]+)", RegexOptions.Multiline);
                var matches = regex.Matches(line);

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        var localAddressAndPort = match.Groups[2].Value;
                        int portNumber = 0;
                        string address = localAddressAndPort;
                        
                        var lastColonIndex = localAddressAndPort.LastIndexOf(':');
                        if (lastColonIndex >= 0 && lastColonIndex < localAddressAndPort.Length - 1)
                        {
                            address = localAddressAndPort.Substring(0, lastColonIndex);
                            int.TryParse(localAddressAndPort.Substring(lastColonIndex + 1), out portNumber);
                        }

                        if (portNumber > 0 && int.TryParse(match.Groups[5].Value, out int pid) && pid > 0)
                        {
                            string processName = GetProcessNameByPid(pid);
                            
                            ports.Add(new SystemPort
                            {
                                Protocol = match.Groups[1].Value,
                                LocalAddress = address,
                                PortNumber = portNumber,
                                State = match.Groups[4].Value,
                                ProcessId = pid,
                                ProcessName = processName
                            });
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        return ports;
    }

    public bool KillProcess(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetProcessNameByPid(int pid)
    {
        try
        {
            var proc = Process.GetProcessById(pid);
            return proc.ProcessName;
        }
        catch
        {
            return "Unknown";
        }
    }
}
