using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace FilePreview.Services;

public class ExplorerService
{
    public string? GetSelectedFilePath()
    {
        var foregroundWindow = Windows.Win32.PInvoke.GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero) return null;

        // CLSID_ShellWindows = {9BA05972-F6A8-11CF-A442-00A0C90A8F39}
        Type? shellWindowsType = Type.GetTypeFromCLSID(new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39"));
        if (shellWindowsType == null) return null;

        dynamic? shellWindows = Activator.CreateInstance(shellWindowsType);
        if (shellWindows == null) return null;

        foreach (dynamic window in shellWindows)
        {
            if (window == null) continue;
            
            try
            {
                if ((IntPtr)window.HWND == foregroundWindow)
                {
                    var shellWindow = window.Document;
                    if (shellWindow != null)
                    {
                        var selectedItems = shellWindow.SelectedItems();
                        if (selectedItems != null && selectedItems.Count > 0)
                        {
                            return selectedItems.Item(0).Path;
                        }
                    }
                }
            }
            catch { /* Ignore COM access errors for specific windows */ }
        }

        // Desktop check
        unsafe
        {
            fixed (char* classNameBuffer = new char[256])
            {
                var pwstr = new Windows.Win32.Foundation.PWSTR(classNameBuffer);
                Windows.Win32.PInvoke.GetClassName(new Windows.Win32.Foundation.HWND(foregroundWindow), pwstr, 256);
                string classNameStr = pwstr.ToString();

                if (classNameStr == "Progman" || classNameStr == "WorkerW")
                {
                    // Desktop handling
                }
            }
        }


        return null;
    }
}
