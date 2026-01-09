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
            long totalSize = files.Sum(f => f.Length);
            
            stackPanel.Children.Add(new TextBlock { Text = $"Contains: {dirs.Length} folders, {files.Length} files", Margin = new Thickness(0, 0, 0, 5) });
            stackPanel.Children.Add(new TextBlock { Text = $"Top-level Size: {totalSize / 1024.0 / 1024.0:F2} MB", Margin = new Thickness(0, 0, 0, 5) });
            stackPanel.Children.Add(new TextBlock { Text = $"Last Modified: {dirInfo.LastWriteTime}", Margin = new Thickness(0, 0, 0, 20) });

            var listView = new System.Windows.Controls.ListView 
            { 
                MaxHeight = 350,
                BorderThickness = new Thickness(0),
                Background = System.Windows.Media.Brushes.Transparent
            };
            
            foreach (var d in dirs.Take(20)) 
            {
                listView.Items.Add(new { Name = d.Name, Type = "Folder", Size = "-", Date = d.LastWriteTime.ToString("yyyy-MM-dd HH:mm") });
            }
            foreach (var f in files.Take(50)) 
            {
                listView.Items.Add(new { Name = f.Name, Type = "File", Size = $"{f.Length / 1024.0:F1} KB", Date = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm") });
            }

            if (dirs.Length + files.Length > 70)
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
        }
        catch (UnauthorizedAccessException)
        {
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Access Denied", Foreground = System.Windows.Media.Brushes.Red });
        }

        return stackPanel;
    }
}
