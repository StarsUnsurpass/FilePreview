using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

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
        using (var process = Process.GetCurrentProcess())
        using (var module = process.MainModule)
        {
            if (module != null)
            {
                var hModule = PInvoke.GetModuleHandle(module.ModuleName);
                _hookHandle = PInvoke.SetWindowsHookEx(Windows.Win32.UI.WindowsAndMessaging.WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _proc, hModule, 0);
            }
        }
    }

    public void Stop()
    {
        _hookHandle?.Dispose();
    }

    private LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0 && (uint)wParam == 0x0100) // WM_KEYDOWN
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (vkCode == 0x20) // VK_SPACE
            {
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
