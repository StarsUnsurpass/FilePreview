using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FilePreview.Previewers;

public class FolderPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        return Directory.Exists(filePath);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var stackPanel = new StackPanel { Margin = new Thickness(20) };
        
        var dirInfo = new DirectoryInfo(filePath);
        
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
        { 
            Text = dirInfo.Name, 
            FontSize = 24, 
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        });

        stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = $"Path: {filePath}", Margin = new Thickness(0, 0, 0, 5) });
        
        try
        {
            var files = dirInfo.GetFiles();
            var dirs = dirInfo.GetDirectories();
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = $"Contains: {dirs.Length} folders, {files.Length} files", Margin = new Thickness(0, 0, 0, 5) });
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = $"Last Modified: {dirInfo.LastWriteTime}", Margin = new Thickness(0, 0, 0, 20) });

            var listView = new System.Windows.Controls.ListView { MaxHeight = 300 };
            foreach (var d in dirs.Take(10)) listView.Items.Add($"[Folder] {d.Name}");
            foreach (var f in files.Take(20)) listView.Items.Add(f.Name);
            
            if (dirs.Length + files.Length > 30)
            {
                listView.Items.Add("...");
            }
            
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Contents (Preview):", FontWeight = FontWeights.SemiBold });
            stackPanel.Children.Add(listView);
        }
        catch (UnauthorizedAccessException)
        {
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Access Denied", Foreground = System.Windows.Media.Brushes.Red });
        }

        return stackPanel;
    }
}
