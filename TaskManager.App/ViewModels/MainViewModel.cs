using System.Collections.ObjectModel;
using System.Timers;
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
    private readonly System.Timers.Timer statsTimer;

    [ObservableProperty]
    private string currentViewTitle;

    [ObservableProperty]
    private ObservableCollection<SystemPort> openPorts;

    [ObservableProperty]
    private ObservableCollection<DockerContainer> dockerContainers;

    [ObservableProperty]
    private SystemStats currentSystemStats;

    [ObservableProperty]
    private bool isPortsViewVisible;

    [ObservableProperty]
    private bool isDockerViewVisible;

    [ObservableProperty]
    private bool isSystemViewVisible;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    partial void OnSearchQueryChanged(string value)
    {
        _ = RefreshCurrentViewAsync();
    }

    public MainViewModel()
    {
        portService = new SystemPortService();
        dockerService = new DockerService();
        monitorService = new SystemMonitorService();

        OpenPorts = new ObservableCollection<SystemPort>();
        DockerContainers = new ObservableCollection<DockerContainer>();
        CurrentSystemStats = new SystemStats();

        currentViewTitle = "// SYSTEM STATS";
        
        IsSystemViewVisible = true;
        IsPortsViewVisible = false;
        IsDockerViewVisible = false;

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
        IsSystemViewVisible = true;
        IsPortsViewVisible = false;
        IsDockerViewVisible = false;
        
        CurrentSystemStats = monitorService.GetStats();
    }

    [RelayCommand]
    private async Task LoadPortsViewAsync()
    {
        CurrentViewTitle = "// OPEN PORTS DETECTED";
        IsSystemViewVisible = false;
        IsPortsViewVisible = true;
        IsDockerViewVisible = false;

        OpenPorts.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;
        var ports = await portService.GetOpenPortsAsync();
        
        foreach (var p in ports)
        {
            if (string.IsNullOrWhiteSpace(query) || 
                p.ProcessName.ToLower().Contains(query) || 
                p.PortNumber.ToString().Contains(query) || 
                p.LocalAddress.ToLower().Contains(query))
            {
                OpenPorts.Add(p);
            }
        }
    }

    [RelayCommand]
    private async Task LoadDockerViewAsync()
    {
        CurrentViewTitle = "// DOCKER CONTAINERS";
        IsSystemViewVisible = false;
        IsPortsViewVisible = false;
        IsDockerViewVisible = true;

        DockerContainers.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;
        var containers = await dockerService.GetContainersAsync();
        
        foreach (var c in containers)
        {
            if (string.IsNullOrWhiteSpace(query) || 
                c.Names.ToLower().Contains(query) || 
                c.Image.ToLower().Contains(query) || 
                c.Status.ToLower().Contains(query))
            {
                DockerContainers.Add(c);
            }
        }
    }

    [RelayCommand]
    private async Task RefreshCurrentViewAsync()
    {
        if (IsPortsViewVisible) await LoadPortsViewAsync();
        else if (IsDockerViewVisible) await LoadDockerViewAsync();
        else LoadSystemView();
    }

    [RelayCommand]
    private async Task KillPortProcessAsync(SystemPort port)
    {
        if (port != null && port.ProcessId > 0)
        {
            portService.KillProcess(port.ProcessId);
            await LoadPortsViewAsync(); 
        }
    }
}
