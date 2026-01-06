using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FilePreview.Previewers;

public class ZipPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return ext == ".zip" || ext == ".nupkg" || ext == ".epub" || ext == ".jar" || ext == ".apk" || ext == ".vsix" || ext == ".xap";
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var stackPanel = new StackPanel { Margin = new Thickness(20) };
        
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
        { 
            Text = Path.GetFileName(filePath), 
            FontSize = 20, 
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        });

        try
        {
            using (var archive = ZipFile.OpenRead(filePath))
            {
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = $"Total entries: {archive.Entries.Count}", Margin = new Thickness(0,0,0,10) });
                
                var listView = new System.Windows.Controls.ListView { MaxHeight = 400 };
                foreach (var entry in archive.Entries.Take(50))
                {
                    listView.Items.Add($"{entry.FullName} ({entry.Length / 1024.0:F1} KB)");
                }
                
                if (archive.Entries.Count > 50)
                {
                    listView.Items.Add("...");
                }
                
                stackPanel.Children.Add(listView);
            }
        }
        catch (Exception ex)
        {
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = $"Error reading zip: {ex.Message}", Foreground = System.Windows.Media.Brushes.Red });
        }

        return stackPanel;
    }
}
