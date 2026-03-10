using System;
using System.Windows;
using System.Windows.Input;

namespace TaskManager.App;

public partial class MainWindow : Window
{
    private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon tb;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModels.MainViewModel();
        StateChanged += MainWindow_StateChanged;

        // Ensure we hook into the exact close event string below
        tb = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon();
        try
        {
            tb.Icon = System.Drawing.SystemIcons.Application;
        }
        catch 
        {
            // fallback if anything goes wrong with SystemIcons
        }
        tb.ToolTipText = "Y2K Developer Tool";
        tb.TrayMouseDoubleClick += Tb_TrayMouseDoubleClick;
        
        // Setup tray context menu via simple code approach
        var menu = new System.Windows.Controls.ContextMenu();
        var openItem = new System.Windows.Controls.MenuItem { Header = "Open" };
        openItem.Click += (s, e) => Tb_TrayMouseDoubleClick(s, e);
        var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitItem.Click += (s, e) => {
            tb.Dispose();
            Application.Current.Shutdown();
        };
        menu.Items.Add(openItem);
        menu.Items.Add(exitItem);
        tb.ContextMenu = menu;
    }

    private void Tb_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        tb?.Dispose();
        Close();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            ShowInTaskbar = false;
        }
        else if (WindowState == WindowState.Maximized)
        {
            BorderThickness = new Thickness(7);
        }
        else
        {
            BorderThickness = new Thickness(0);
        }
    }
}