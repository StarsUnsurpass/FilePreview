using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace FilePreview.Previewers;

public class PdfPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() == ".pdf";
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var webView = new WebView2();
        InitializeWebView(webView, filePath);
        return webView;
    }

    private async void InitializeWebView(WebView2 webView, string filePath)
    {
        var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FilePreview", "WebView2");
        Directory.CreateDirectory(userDataFolder);
        var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await webView.EnsureCoreWebView2Async(env);
        webView.CoreWebView2.Navigate(new Uri(filePath).AbsoluteUri);
    }
}
