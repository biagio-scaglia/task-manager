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
}
