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
        
        var statusText = new TextBlock { Text = "Calculating folder statistics...", Margin = new Thickness(0, 0, 0, 5), FontStyle = FontStyles.Italic };
        stackPanel.Children.Add(statusText);

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                var files = dirInfo.GetFiles();
                var dirs = dirInfo.GetDirectories();
                long totalSize = files.Sum(f => f.Length);

                var dirList = dirs.Take(20).Select(d => new { Name = d.Name, Type = "Folder", Size = "-", Date = d.LastWriteTime.ToString("yyyy-MM-dd HH:mm") }).ToList();
                var fileList = files.Take(50).Select(f => new { Name = f.Name, Type = "File", Size = $"{f.Length / 1024.0:F1} KB", Date = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm") }).ToList();
                bool hasMore = (dirs.Length + files.Length) > 70;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    stackPanel.Children.Remove(statusText);
                    stackPanel.Children.Add(new TextBlock { Text = $"Contains: {dirs.Length} folders, {files.Length} files", Margin = new Thickness(0, 0, 0, 5) });
                    stackPanel.Children.Add(new TextBlock { Text = $"Top-level Size: {totalSize / 1024.0 / 1024.0:F2} MB", Margin = new Thickness(0, 0, 0, 5) });
                    stackPanel.Children.Add(new TextBlock { Text = $"Last Modified: {dirInfo.LastWriteTime}", Margin = new Thickness(0, 0, 0, 20) });

                    var listView = new System.Windows.Controls.ListView 
                    { 
                        MaxHeight = 350,
                        BorderThickness = new Thickness(0),
                        Background = System.Windows.Media.Brushes.Transparent
                    };
                    
                    foreach (var item in dirList) listView.Items.Add(item);
                    foreach (var item in fileList) listView.Items.Add(item);

                    if (hasMore)
                    {
                        listView.Items.Add(new { Name = "...", Type = "", Size = "", Date = "" });
                    }

                    var gridView = new System.Windows.Controls.GridView();
                    gridView.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "Name", DisplayMemberBinding = new System.Windows.Data.Binding("Name"), Width = 300 });
                    gridView.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "Size", DisplayMemberBinding = new System.Windows.Data.Binding("Size"), Width = 80 });
                    gridView.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "Date", DisplayMemberBinding = new System.Windows.Data.Binding("Date"), Width = 150 });
                    listView.View = gridView;
                    
                    stackPanel.Children.Add(new TextBlock { Text = "Contents:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,5) });
                    stackPanel.Children.Add(listView);
                });
            }
            catch (UnauthorizedAccessException)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    statusText.Text = "Access Denied";
                    statusText.Foreground = System.Windows.Media.Brushes.Red;
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    statusText.Text = $"Error: {ex.Message}";
                    statusText.Foreground = System.Windows.Media.Brushes.Red;
                });
            }
        });

        return stackPanel;
    }
}
