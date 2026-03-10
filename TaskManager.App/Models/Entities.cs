using System;

namespace TaskManager.App.Models;

public class SystemPort
{
    public required string Protocol { get; set; }
    public required string LocalAddress { get; set; }
    public required int PortNumber { get; set; }
    public required string State { get; set; }
    public required int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
}

public class DockerContainer
{
    public required string Id { get; set; }
    public required string Image { get; set; }
    public required string Command { get; set; }
    public required string Created { get; set; }
    public required string Status { get; set; }
    public required string Ports { get; set; }
    public required string Names { get; set; }
}

public class SystemStats
{
    public double CpuUsagePercentage { get; set; }
    public double RamUsagePercentage { get; set; }
    public double TotalRamGB { get; set; }
    public double UsedRamGB { get; set; }
}
