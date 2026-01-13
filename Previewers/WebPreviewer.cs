using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace FilePreview.Previewers;

public class WebPreviewer : IPreviewer
{
    private static readonly string[] Extensions = { ".html", ".htm", ".svg", ".mhtml" };

    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return Extensions.Contains(ext);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var webView = new WebView2();
        InitializeWebView(webView, filePath);
        return webView;
    }

    private async void InitializeWebView(WebView2 webView, string filePath)
    {
        try
        {
            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FilePreview", "WebView2");
            Directory.CreateDirectory(userDataFolder);
            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView.EnsureCoreWebView2Async(env);
            webView.CoreWebView2.Navigate(new Uri(filePath).AbsoluteUri);
        }
        catch (Exception)
        {
            // Handle initialization errors silently or show a placeholder?
            // For now, we rely on the control to just not render if it fails, or maybe we should add error handling logic.
            // But preserving original simple behavior is safer.
        }
    }
}