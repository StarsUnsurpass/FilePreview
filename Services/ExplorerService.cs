using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Serilog;

namespace FilePreview.Services;

public class ExplorerService
{
    public string? GetSelectedFilePath()
    {
        try
        {
            var foregroundWindow = Windows.Win32.PInvoke.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero) 
            {
                Log.Debug("GetSelectedFilePath: No foreground window.");
                return null;
            }

            // CLSID_ShellWindows = {9BA05972-F6A8-11CF-A442-00A0C90A8F39}
            Type? shellWindowsType = Type.GetTypeFromCLSID(new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39"));
            if (shellWindowsType == null) 
            {
                Log.Error("GetSelectedFilePath: Failed to get ShellWindows type.");
                return null;
            }

            dynamic? shellWindows = Activator.CreateInstance(shellWindowsType);
            if (shellWindows == null) 
            {
                Log.Error("GetSelectedFilePath: Failed to create ShellWindows instance.");
                return null;
            }

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
                                string path = selectedItems.Item(0).Path;
                                Log.Debug("GetSelectedFilePath: Found path {Path}", path);
                                return path;
                            }
                        }
                    }
                }
                catch (Exception ex)
                { 
                    // Expected to fail for some windows (like IE tabs, non-explorer windows)
                    Log.Verbose(ex, "GetSelectedFilePath: Error accessing window property.");
                }
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
                        // Desktop handling - TODO: Implement if needed
                        Log.Debug("GetSelectedFilePath: Desktop is foreground, handling not implemented.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetSelectedFilePath: Unhandled error.");
        }

        return null;
    }
}
