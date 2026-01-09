using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FilePreview.Previewers;

public class FontPreviewer : IPreviewer
{
    private static readonly string[] Extensions = { ".ttf", ".otf", ".woff", ".woff2" };

    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return Extensions.Contains(ext);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stackPanel = new StackPanel { Margin = new Thickness(20) };

        try
        {
            var fontFamily = new System.Windows.Media.FontFamily(new Uri(filePath), "./#" + GetFontName(filePath));
            
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = Path.GetFileName(filePath), 
                FontSize = 24, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            });

            string sampleText = "The quick brown fox jumps over the lazy dog. 1234567890\n" +
                                "敏捷的棕色狐狸跨过懒惰的狗。";

            double[] sizes = { 12, 18, 24, 36, 48, 72 };

            foreach (var size in sizes)
            {
                stackPanel.Children.Add(new TextBlock { Text = $"Size {size}pt", FontSize = 12, Foreground = System.Windows.Media.Brushes.Gray, Margin = new Thickness(0, 10, 0, 0) });
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = sampleText, 
                    FontSize = size, 
                    FontFamily = fontFamily,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 10)
                });
            }
        }
        catch (Exception ex)
        {
            stackPanel.Children.Add(new TextBlock { Text = $"Error loading font: {ex.Message}", Foreground = System.Windows.Media.Brushes.Red });
        }

        scrollViewer.Content = stackPanel;
        return scrollViewer;
    }

    private string GetFontName(string filePath)
    {
        // This is a bit simplified. WPF FontFamily usually needs the font name inside the file.
        // For some files, using the filename works if it matches the internal name.
        // A better way would be using GlyphTypeface but it's more complex.
        return ""; 
    }
}
