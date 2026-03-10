using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Timers;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManager.App.Models;
using TaskManager.App.Services;

namespace TaskManager.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SystemPortService portService;
    private readonly DockerService dockerService;
    private readonly SystemMonitorService monitorService;
    private readonly ThemeService themeService;
    private readonly HostsFileService hostsFileService;
    private readonly StartupManagerService startupService;
    private readonly System.Timers.Timer statsTimer;

    private List<SystemPort>? _cachedPorts;
    private List<DockerContainer>? _cachedContainers;

    [ObservableProperty]
    private string currentViewTitle;

    [ObservableProperty]
    private ObservableCollection<SystemPort> openPorts;

    [ObservableProperty]
    private ObservableCollection<DockerContainer> dockerContainers;

    [ObservableProperty]
    private ObservableCollection<HostEntry> hostEntries;

    [ObservableProperty]
    private ObservableCollection<StartupEntry> startupEntries;

    [ObservableProperty]
    private SystemStats currentSystemStats;

    [ObservableProperty]
    private AppView currentView;

    [ObservableProperty]
    private bool isAdmin;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    partial void OnSearchQueryChanged(string value)
    {
        _ = RefreshCurrentViewAsync();
    }

    public MainViewModel()
    {
        portService = new SystemPortService();
        dockerService = new DockerService();
        monitorService = new SystemMonitorService();
        themeService = new ThemeService();
        hostsFileService = new HostsFileService();
        startupService = new StartupManagerService();

        OpenPorts = new ObservableCollection<SystemPort>();
        DockerContainers = new ObservableCollection<DockerContainer>();
        HostEntries = new ObservableCollection<HostEntry>();
        StartupEntries = new ObservableCollection<StartupEntry>();
        CurrentSystemStats = new SystemStats();

        currentViewTitle = "// SYSTEM STATS";
        CurrentView = AppView.System;

        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        statsTimer = new System.Timers.Timer(2000);
        statsTimer.Elapsed += StatsTimer_Elapsed;
        statsTimer.Start();
        
        LoadSystemViewCommand.Execute(null);
    }

    private void StatsTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        var stats = monitorService.GetStats();
        App.Current.Dispatcher.Invoke(() =>
        {
            CurrentSystemStats = stats;
        });
    }

    [RelayCommand]
    private void LoadSystemView()
    {
        CurrentViewTitle = "// SYSTEM STATS";
        CurrentView = AppView.System;
        
        CurrentSystemStats = monitorService.GetStats();
    }

    [RelayCommand]
    private async Task LoadPortsViewAsync()
    {
        CurrentViewTitle = "// OPEN PORTS DETECTED";
        CurrentView = AppView.Ports;

        IsLoading = true;
        OpenPorts.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;
        
        if (_cachedPorts == null)
        {
            _cachedPorts = await portService.GetOpenPortsAsync();
        }
        
        foreach (var p in _cachedPorts)
        {
            if (string.IsNullOrWhiteSpace(query) || 
                p.ProcessName.ToLower().Contains(query) || 
                p.PortNumber.ToString().Contains(query) || 
                p.LocalAddress.ToLower().Contains(query))
            {
                OpenPorts.Add(p);
            }
        }
        IsLoading = false;
    }

    [RelayCommand]
    private async Task LoadDockerViewAsync()
    {
        CurrentViewTitle = "// DOCKER CONTAINERS";
        CurrentView = AppView.Docker;

        IsLoading = true;
        DockerContainers.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;
        
        if (_cachedContainers == null)
        {
            _cachedContainers = await dockerService.GetContainersAsync();
        }
        
        foreach (var c in _cachedContainers)
        {
            if (string.IsNullOrWhiteSpace(query) || 
                c.Names.ToLower().Contains(query) || 
                c.Image.ToLower().Contains(query) || 
                c.Status.ToLower().Contains(query))
            {
                DockerContainers.Add(c);
            }
        }
        IsLoading = false;
    }

    [RelayCommand]
    private async Task RefreshCurrentViewAsync()
    {
        switch (CurrentView)
        {
            case AppView.Ports:
                await LoadPortsViewAsync();
                break;
            case AppView.Docker:
                await LoadDockerViewAsync();
                break;
            case AppView.Settings:
                LoadSettingsView();
                break;
            case AppView.Hosts:
                await LoadHostsViewAsync();
                break;
            case AppView.Startup:
                await LoadStartupViewAsync();
                break;
            case AppView.Info:
                LoadInfoView();
                break;
            default:
                LoadSystemView();
                break;
        }
    }

    [RelayCommand]
    private void LoadSettingsView()
    {
        CurrentViewTitle = "// SYSTEM SETTINGS";
        CurrentView = AppView.Settings;
    }

    [RelayCommand]
    private void LoadInfoView()
    {
        CurrentViewTitle = "// INFO & DOCUMENTATION";
        CurrentView = AppView.Info;
    }

    [RelayCommand]
    private async Task LoadHostsViewAsync()
    {
        CurrentViewTitle = "// HOSTS FILE ENTRIES";
        CurrentView = AppView.Hosts;

        IsLoading = true;
        HostEntries.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;

        var entries = await hostsFileService.GetHostsEntriesAsync();
        
        foreach (var entry in entries)
        {
            if (entry.IsCommentOnly && !string.IsNullOrWhiteSpace(query))
                continue; // Skip comments if searching

            if (string.IsNullOrWhiteSpace(query) || 
                entry.Hostname.ToLower().Contains(query) || 
                entry.IPAddress.Contains(query))
            {
                HostEntries.Add(entry);
            }
        }
        IsLoading = false;
    }

    [RelayCommand]
    private async Task SaveHostsFileAsync()
    {
        if (IsAdmin)
        {
            await hostsFileService.SaveHostsEntriesAsync(HostEntries);
            await LoadHostsViewAsync();
        }
    }

    [RelayCommand]
    private async Task LoadStartupViewAsync()
    {
        CurrentViewTitle = "// STARTUP MANAGER";
        CurrentView = AppView.Startup;

        IsLoading = true;
        StartupEntries.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;

        var entries = await startupService.GetStartupEntriesAsync();
        
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(query) || 
                entry.Name.ToLower().Contains(query) || 
                entry.Command.ToLower().Contains(query))
            {
                StartupEntries.Add(entry);
            }
        }
        IsLoading = false;
    }

    [RelayCommand]
    private async Task DeleteStartupEntryAsync(StartupEntry entry)
    {
        if (entry != null)
        {
            bool success = await startupService.DeleteStartupEntryAsync(entry);
            if (success)
            {
                await LoadStartupViewAsync();
            }
        }
    }

    [RelayCommand]
    private void ChangeTheme(string themeName)
    {
        if (!string.IsNullOrWhiteSpace(themeName))
        {
            themeService.ChangeTheme(themeName);
        }
    }

    [RelayCommand]
    private async Task KillPortProcessAsync(SystemPort port)
    {
        if (port != null && port.ProcessId > 0)
        {
            portService.KillProcess(port.ProcessId);
            _cachedPorts = null; // Invalidate cache so it forces a re-scan next load
            await LoadPortsViewAsync(); 
        }
    }

    [RelayCommand]
    private void RestartAsAdmin()
    {
        var exeName = Process.GetCurrentProcess().MainModule?.FileName;
        if (exeName == null) return;
        
        ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
        {
            UseShellExecute = true,
            Verb = "runas"
        };
        try
        {
            Process.Start(startInfo);
            Application.Current.Shutdown();
        }
        catch 
        {
            // User likely cancelled the UAC prompt
        }
    }

    [RelayCommand]
    private async Task StartDockerContainerAsync(DockerContainer container)
    {
        if (container != null)
        {
            bool success = dockerService.StartContainer(container.Id);
            if (success) 
            {
                _cachedContainers = null;
                await LoadDockerViewAsync();
            }
        }
    }

    [RelayCommand]
    private async Task StopDockerContainerAsync(DockerContainer container)
    {
        if (container != null)
        {
            bool success = dockerService.StopContainer(container.Id);
            if (success) 
            {
                _cachedContainers = null;
                await LoadDockerViewAsync();
            }
        }
    }

    [RelayCommand]
    private async Task RemoveDockerContainerAsync(DockerContainer container)
    {
        if (container != null)
        {
            bool success = dockerService.RemoveContainer(container.Id);
            if (success) 
            {
                _cachedContainers = null;
                await LoadDockerViewAsync();
            }
        }
    }
}

