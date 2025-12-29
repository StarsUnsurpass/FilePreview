using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using Markdig;

namespace FilePreview.Previewers;

public class MarkdownPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return ext == ".md" || ext == ".markdown";
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
        
        var content = File.ReadAllText(filePath);
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(content, pipeline);

        // Add some CSS to make it look good in dark mode
        var styledHtml = $@"
<html>
<head>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            padding: 20px;
            background-color: #1e1e1e;
            color: #d4d4d4;
            line-height: 1.6;
        }}
        pre {{
            background-color: #2d2d2d;
            padding: 10px;
            border-radius: 5px;
            overflow-x: auto;
        }}
        code {{
            font-family: 'Consolas', monospace;
        }}
        img {{
            max-width: 100%;
        }}
        a {{
            color: #3794ff;
        }}
    </style>
</head>
<body>
    {html}
</body>
</html>";

        webView.NavigateToString(styledHtml);
    }
}
