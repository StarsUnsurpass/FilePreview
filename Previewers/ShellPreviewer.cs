using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace FilePreview.Previewers;

// This is a simplified version of a Shell Preview Host
public class ShellPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        // This is a fallback previewer. We only use it if it has a registered preview handler.
        return GetPreviewHandlerGuid(filePath) != null;
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var host = new PreviewHandlerHost();
        host.LoadFile(filePath);
        return host;
    }

    private string? GetPreviewHandlerGuid(string filePath)
    {
        var extension = System.IO.Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension)) return null;

        using (var key = Registry.ClassesRoot.OpenSubKey(extension))
        {
            if (key == null) return null;
            using (var shellex = key.OpenSubKey("ShellEx"))
            {
                if (shellex == null) return null;
                using (var preview = shellex.OpenSubKey("{8895b1c6-b41f-4c1c-a562-0d564250836f}"))
                {
                    return preview?.GetValue("")?.ToString();
                }
            }
        }
    }
}

public class PreviewHandlerHost : HwndHost
{
    private IPreviewHandler? _previewHandler;
    private string? _filePath;

    public void LoadFile(string filePath)
    {
        _filePath = filePath;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        var guid = GetPreviewHandlerGuid(_filePath!);
        if (guid == null) return new HandleRef(this, IntPtr.Zero);

        var type = Type.GetTypeFromCLSID(new Guid(guid));
        _previewHandler = (IPreviewHandler?)Activator.CreateInstance(type!);

        if (_previewHandler is IInitializeWithFile initFile)
        {
            initFile.Initialize(_filePath!, 0);
        }
        else if (_previewHandler is IInitializeWithItem initItem)
        {
            // Implementation for IInitializeWithItem omitted for brevity
        }

        var rect = new RECT { left = 0, top = 0, right = (int)ActualWidth, bottom = (int)ActualHeight };
        _previewHandler!.SetWindow(hwndParent.Handle, ref rect);
        _previewHandler.DoPreview();

        return new HandleRef(this, hwndParent.Handle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        _previewHandler?.Unload();
        if (_previewHandler != null) Marshal.ReleaseComObject(_previewHandler);
    }

    private string? GetPreviewHandlerGuid(string filePath)
    {
        var extension = System.IO.Path.GetExtension(filePath);
        using (var key = Registry.ClassesRoot.OpenSubKey(extension))
        {
            using (var preview = key?.OpenSubKey("ShellEx")?.OpenSubKey("{8895b1c6-b41f-4c1c-a562-0d564250836f}"))
            {
                return preview?.GetValue("")?.ToString();
            }
        }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8895b1c6-b41f-4c1c-a562-0d564250836f")]
    interface IPreviewHandler
    {
        void SetWindow(IntPtr hwnd, ref RECT rect);
        void SetRect(ref RECT rect);
        void DoPreview();
        void Unload();
        void SetFocus();
        void QueryFocus(out IntPtr phwnd);
        [PreserveSig]
        uint TranslateAccelerator(ref MSG pmsg);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("b7d14566-0509-4cce-a71f-0a554233bd9b")]
    interface IInitializeWithFile
    {
        void Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszFilePath, uint grfMode);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7f73b541-ccb1-444b-974d-9923dd4073d8")]
    interface IInitializeWithItem
    {
        void Initialize(object psi, uint grfMode);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RECT { public int left, top, right, bottom; }
    [StructLayout(LayoutKind.Sequential)]
    struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int ptX; public int ptY; }
}
