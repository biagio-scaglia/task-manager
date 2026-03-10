using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TaskManager.App.Services;

public class HostEntry
{
    public string IPAddress { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string OriginalLine { get; set; } = string.Empty;
    public bool IsCommentOnly { get; set; }
}

public class HostsFileService
{
    private static readonly string HostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

    public async Task<List<HostEntry>> GetHostsEntriesAsync()
    {
        var entries = new List<HostEntry>();
        
        if (!File.Exists(HostsFilePath)) return entries;

        var lines = await File.ReadAllLinesAsync(HostsFilePath);
        
        foreach (var line in lines)
        {
            var entry = ParseLine(line);
            entries.Add(entry);
        }

        return entries;
    }

    public async Task SaveHostsEntriesAsync(IEnumerable<HostEntry> entries)
    {
        var lines = entries.Select(e => BuildLine(e)).ToList();
        await File.WriteAllLinesAsync(HostsFilePath, lines);
    }

    private HostEntry ParseLine(string line)
    {
        var entry = new HostEntry { OriginalLine = line };
        var trimmed = line.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            entry.IsCommentOnly = true;
            return entry;
        }

        bool isCommented = trimmed.StartsWith("#");
        var contentToParse = isCommented ? trimmed.Substring(1).Trim() : trimmed;

        // Splitta su qualsiasi whitespace
        var parts = Regex.Split(contentToParse, @"\s+");

        if (parts.Length >= 2 && IsValidIP(parts[0]))
        {
            entry.IPAddress = parts[0];
            entry.Hostname = string.Join(" ", parts.Skip(1));
            entry.IsActive = !isCommented;
        }
        else
        {
            entry.IsCommentOnly = true;
        }

        return entry;
    }

    private string BuildLine(HostEntry entry)
    {
        if (entry.IsCommentOnly)
        {
            return entry.OriginalLine; // Preserve empty lines and pure comments entirely
        }

        string prefix = entry.IsActive ? "" : "# ";
        return $"{prefix}{entry.IPAddress}\t{entry.Hostname}";
    }

    private bool IsValidIP(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out _);
    }
}
