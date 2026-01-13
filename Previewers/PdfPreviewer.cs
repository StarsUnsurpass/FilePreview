using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace FilePreview.Previewers;

public class PdfPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        return string.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var webView = new WebView2();
        InitializeAsync(webView, filePath);
        return webView;
    }

    private async void InitializeAsync(WebView2 webView, string filePath)
    {
        try
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate(filePath);
        }
        catch (Exception)
        {
            // Handle initialization error
        }
    }
}
