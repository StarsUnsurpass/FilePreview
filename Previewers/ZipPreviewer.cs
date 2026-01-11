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

        var statusText = new System.Windows.Controls.TextBlock { Text = "Loading archive contents...", Margin = new Thickness(0, 0, 0, 10) };
        stackPanel.Children.Add(statusText);

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    var entryCount = archive.Entries.Count;
                    var entries = archive.Entries.Take(50).Select(e => $"{e.FullName} ({e.Length / 1024.0:F1} KB)").ToList();
                    var hasMore = entryCount > 50;

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        stackPanel.Children.Remove(statusText);
                        stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = $"Total entries: {entryCount}", Margin = new Thickness(0, 0, 0, 10) });

                        var listView = new System.Windows.Controls.ListView { MaxHeight = 400 };
                        foreach (var item in entries)
                        {
                            listView.Items.Add(item);
                        }

                        if (hasMore)
                        {
                            listView.Items.Add("...");
                        }

                        stackPanel.Children.Add(listView);
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    statusText.Text = $"Error reading zip: {ex.Message}";
                    statusText.Foreground = System.Windows.Media.Brushes.Red;
                });
            }
        });

        return stackPanel;
    }
}
