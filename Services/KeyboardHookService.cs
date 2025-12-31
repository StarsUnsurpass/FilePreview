using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Serilog;

namespace FilePreview.Services;

public class KeyboardHookService : IDisposable
{
    private UnhookWindowsHookExSafeHandle? _hookHandle;
    private HOOKPROC _proc;
    public event Action? SpacePressed;

    public KeyboardHookService()
    {
        _proc = HookCallback;
    }

    public void Start()
    {
        try
        {
            Log.Information("Starting KeyboardHookService...");
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                if (module != null)
                {
                    var hModule = PInvoke.GetModuleHandle(module.ModuleName);
                    _hookHandle = PInvoke.SetWindowsHookEx(Windows.Win32.UI.WindowsAndMessaging.WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _proc, hModule, 0);
                    
                    if (_hookHandle == null || _hookHandle.IsInvalid)
                    {
                         Log.Error("Failed to install keyboard hook. Error code: {ErrorCode}", Marshal.GetLastWin32Error());
                    }
                    else
                    {
                         Log.Information("Keyboard hook installed successfully.");
                    }
                }
                else
                {
                    Log.Error("Failed to get main module.");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error starting KeyboardHookService.");
        }
    }

    public void Stop()
    {
        Log.Information("Stopping KeyboardHookService...");
        _hookHandle?.Dispose();
    }

    private LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && (uint)wParam == 0x0100) // WM_KEYDOWN
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (vkCode == 0x20) // VK_SPACE
            {
                Log.Debug("Space key detected by hook.");
                SpacePressed?.Invoke();
                // If we want to prevent the space from being processed by the system (e.g. scrolling in explorer)
                // return new LRESULT(1);
            }
        }
        return PInvoke.CallNextHookEx(null, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Stop();
    }
}
