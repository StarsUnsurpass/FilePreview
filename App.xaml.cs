using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using FilePreview.Services;
using Serilog;

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
        // Configure Serilog
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Application Starting...");

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled Exception");
        };

        _mutex = new System.Threading.Mutex(true, "FilePreview-SingleInstance-Mutex", out bool createdNew);
        if (!createdNew)
        {
            Log.Warning("Another instance is already running. Shutting down.");
            System.Windows.Application.Current.Shutdown();
            return;
        }

        base.OnStartup(e);

        // Prevent app from closing when window is closed (Tray app behavior)
        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try 
        {
            // Pre-initialize window for faster performance
            _mainWindow = new MainWindow();
            // Ensure it's handled correctly
            _mainWindow.ShowActivated = false;
            // We don't show it yet
            
            _explorerService = new ExplorerService();
            _keyboardHook = new KeyboardHookService();
            _keyboardHook.SpacePressed += OnSpacePressed;
            _keyboardHook.Start();

            SetupTrayIcon();
            Log.Information("Services initialized successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing services.");
        }
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
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load tray icon, using default.");
        }

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
                _mainWindow.Show();
                // To trigger About dialog we might need a public method or just show the window
                // Since About is a menu item inside the window, just showing the window is enough interaction for now.
                // Or we can simulate a click if strictly needed, but simply showing is better UX here.
            });
        });
        contextMenu.Items.Add("-"); // Separator
        contextMenu.Items.Add("退出 (Exit)", null, (s, e) => Shutdown());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void OnSpacePressed()
    {
        Log.Debug("Space key event received.");
        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                var selectedFile = _explorerService?.GetSelectedFilePath();
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    Log.Information("Selected file for preview: {FilePath}", selectedFile);
                    Dispatcher.Invoke(() =>
                    {
                        if (_mainWindow == null)
                        {
                            _mainWindow = new MainWindow();
                        }

                        if (_mainWindow.IsVisible && _mainWindow.Tag?.ToString() == selectedFile)
                        {
                            Log.Debug("Hiding preview for: {FilePath}", selectedFile);
                            _mainWindow.HidePreview();
                        }
                        else
                        {
                            Log.Debug("Showing preview for: {FilePath}", selectedFile);
                            _mainWindow.Tag = selectedFile;
                            _mainWindow.ShowPreview(selectedFile);
                        }
                    });
                }
                else
                {
                    Log.Debug("No file selected or unable to retrieve file path.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling space press.");
            }
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application Exiting...");
        _keyboardHook?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        _notifyIcon?.Dispose();
        base.OnExit(e);
        Log.CloseAndFlush();
    }
}