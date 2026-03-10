using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace TaskManager.App.Services;

public class StartupEntry
{
    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string RegistryLocation { get; set; } = string.Empty; // e.g. "HKCU" or "HKLM"
    
    // Security flags
    public bool IsSystemExecutable { get; set; }
    public bool RequiresAdmin { get; set; }
}

public class StartupManagerService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public async Task<List<StartupEntry>> GetStartupEntriesAsync()
    {
        return await Task.Run(() =>
        {
            var entries = new List<StartupEntry>();

            // Read Current User
            entries.AddRange(ReadRegistryKey(Registry.CurrentUser, RunKeyPath, "HKCU"));

            // Read Local Machine
            entries.AddRange(ReadRegistryKey(Registry.LocalMachine, RunKeyPath, "HKLM"));

            return entries;
        });
    }

    private IEnumerable<StartupEntry> ReadRegistryKey(RegistryKey baseKey, string subKeyPath, string location)
    {
        var entries = new List<StartupEntry>();

        try
        {
            using var key = baseKey.OpenSubKey(subKeyPath, writable: false);
            if (key != null)
            {
                foreach (var valueName in key.GetValueNames())
                {
                    var valueData = key.GetValue(valueName)?.ToString() ?? string.Empty;
                    bool isSystem = IsSystemPath(valueData);
                    
                    entries.Add(new StartupEntry
                    {
                        Name = valueName,
                        Command = valueData,
                        RegistryLocation = location,
                        IsSystemExecutable = isSystem,
                        RequiresAdmin = (location == "HKLM")
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle access denied exceptions silently for the UI
            Console.WriteLine($"Error reading {location} startup keys: {ex.Message}");
        }

        return entries;
    }

    public async Task<bool> DeleteStartupEntryAsync(StartupEntry entry)
    {
        return await Task.Run(() =>
        {
            // Prevent deletion of obvious system binaries as a safety measure
            if (entry.IsSystemExecutable)
            {
                return false;
            }

            try
            {
                RegistryKey baseKey = entry.RegistryLocation == "HKCU" ? Registry.CurrentUser : Registry.LocalMachine;
                using var key = baseKey.OpenSubKey(RunKeyPath, writable: true);
                if (key != null)
                {
                    key.DeleteValue(entry.Name, throwOnMissingValue: false);
                    return true;
                }
            }
            catch
            {
                // Typically Access Denied if trying to write HKLM without admin
                return false;
            }

            return false;
        });
    }

    private bool IsSystemPath(string command)
    {
        var lowerCommand = command.ToLowerInvariant();
        // Guard against removing core OS processes accidentally
        return lowerCommand.Contains(@"c:\windows\system32") || 
               lowerCommand.Contains("cmd.exe") || 
               lowerCommand.Contains("explorer.exe");
    }
}
