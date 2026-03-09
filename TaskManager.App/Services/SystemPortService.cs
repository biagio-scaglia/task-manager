using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TaskManager.App.Models;

namespace TaskManager.App.Services;

public class SystemPortService
{
    public async Task<List<SystemPort>> GetOpenPortsAsync()
    {
        return await Task.Run(() =>
        {
            var ports = new List<SystemPort>();
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
                
                // Active TCP Connections
                foreach (var c in connections)
                {
                    ports.Add(new SystemPort
                    {
                        Protocol = "TCP",
                        LocalAddress = c.LocalEndPoint.Address.ToString(),
                        PortNumber = c.LocalEndPoint.Port,
                        State = c.State.ToString(),
                        ProcessId = 0, 
                        ProcessName = "SYS.NET.TCP" 
                    });
                }
                
                // TCP Listeners
                var tcpListeners = properties.GetActiveTcpListeners();
                foreach (var l in tcpListeners)
                {
                    ports.Add(new SystemPort
                    {
                        Protocol = "TCP",
                        LocalAddress = l.Address.ToString(),
                        PortNumber = l.Port,
                        State = "Listening",
                        ProcessId = 0,
                        ProcessName = "SYS.NET.LISTENER"
                    });
                }
                
                // UDP Listeners
                var udpListeners = properties.GetActiveUdpListeners();
                foreach (var u in udpListeners)
                {
                    ports.Add(new SystemPort
                    {
                        Protocol = "UDP",
                        LocalAddress = u.Address.ToString(),
                        PortNumber = u.Port,
                        State = "Listening",
                        ProcessId = 0,
                        ProcessName = "SYS.NET.UDP"
                    });
                }
            }
            catch
            {
            }
            return ports;
        });
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
