using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.App.Models;

namespace TaskManager.App.Services;

public class DockerService
{
    public async Task<List<DockerContainer>> GetContainersAsync()
    {
        var containers = new List<DockerContainer>();
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "ps -a --format \"{{json .}}\"",
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
                string output = await reader.ReadToEndAsync();

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;
                        
                        containers.Add(new DockerContainer
                        {
                            Id = root.GetProperty("ID").GetString() ?? string.Empty,
                            Image = root.GetProperty("Image").GetString() ?? string.Empty,
                            Command = root.GetProperty("Command").GetString() ?? string.Empty,
                            Created = root.GetProperty("CreatedAt").GetString() ?? string.Empty,
                            Status = root.GetProperty("Status").GetString() ?? string.Empty,
                            Ports = root.GetProperty("Ports").GetString() ?? string.Empty,
                            Names = root.GetProperty("Names").GetString() ?? string.Empty
                        });
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }

        return containers;
    }

    public bool StartContainer(string containerId)
    {
        return RunDockerCommand($"start {containerId}");
    }

    public bool StopContainer(string containerId)
    {
        return RunDockerCommand($"stop {containerId}");
    }

    public bool RemoveContainer(string containerId)
    {
        return RunDockerCommand($"rm -f {containerId}");
    }

    private bool RunDockerCommand(string args)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(processStartInfo);
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
