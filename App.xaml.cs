using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using FilePreview.Services;

namespace FilePreview;

public partial class App : System.Windows.Application
{
    private KeyboardHookService? _keyboardHook;
    private ExplorerService? _explorerService;
    private MainWindow? _mainWindow;
    private System.Threading.Mutex? _mutex;
    private NotifyIcon? _notifyIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new System.Threading.Mutex(true, "FilePreview-SingleInstance-Mutex", out bool createdNew);
        if (!createdNew)
        {
            System.Windows.Application.Current.Shutdown();
            return;
        }

        base.OnStartup(e);

        _explorerService = new ExplorerService();
        _keyboardHook = new KeyboardHookService();
        _keyboardHook.SpacePressed += OnSpacePressed;
        _keyboardHook.Start();

        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        Icon appIcon = SystemIcons.Application;
        try
        {
            var iconUri = new Uri("pack://application:,,,/app.ico");
            var streamResourceInfo = System.Windows.Application.GetResourceStream(iconUri);
            if (streamResourceInfo != null)
            {
                appIcon = new Icon(streamResourceInfo.Stream);
            }
        }
        catch { /* Fallback to default icon */ }

        _notifyIcon = new NotifyIcon
        {
            Icon = appIcon,
            Text = "FilePreview",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("关于 (About)", null, (s, e) => 
        {
            Dispatcher.Invoke(() => 
            {
                if (_mainWindow == null) _mainWindow = new MainWindow();
                // We show a dummy preview or just call the about logic
                _mainWindow.Show();
                // Manually trigger about if needed, or just let them use the menu in window
                // Better: we can add a public method to show about
            });
        });
        contextMenu.Items.Add("-"); // Separator
        contextMenu.Items.Add("退出 (Exit)", null, (s, e) => Shutdown());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void OnSpacePressed()
    {
        System.Threading.Tasks.Task.Run(() =>
        {
            var selectedFile = _explorerService?.GetSelectedFilePath();
            if (!string.IsNullOrEmpty(selectedFile))
            {
                Dispatcher.Invoke(() =>
                {
                    if (_mainWindow == null)
                    {
                        _mainWindow = new MainWindow();
                    }

                    if (_mainWindow.IsVisible && _mainWindow.Tag?.ToString() == selectedFile)
                    {
                        _mainWindow.HidePreview();
                    }
                    else
                    {
                        _mainWindow.Tag = selectedFile;
                        _mainWindow.ShowPreview(selectedFile);
                    }
                });
            }
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _keyboardHook?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}