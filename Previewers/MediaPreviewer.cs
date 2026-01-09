using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace FilePreview.Previewers;

public class MediaPreviewer : IPreviewer
{
    private static readonly string[] VideoExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".webm" };
    private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".flac", ".ogg", ".wma", ".m4a", ".aac" };

    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return VideoExtensions.Contains(ext) || AudioExtensions.Contains(ext);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var grid = new Grid();
        
        var mediaElement = new MediaElement
        {
            Source = new Uri(filePath),
            LoadedBehavior = MediaState.Manual,
            UnloadedBehavior = MediaState.Stop,
            Stretch = Stretch.Uniform
        };

        grid.Children.Add(mediaElement);

        var playIcon = new SymbolIcon(SymbolRegular.Play24);
        var pauseIcon = new SymbolIcon(SymbolRegular.Pause24);

        var playButton = new Wpf.Ui.Controls.Button 
        { 
            Icon = pauseIcon,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center, 
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0,0,0,20),
            Padding = new Thickness(15, 10, 15, 10),
            Appearance = ControlAppearance.Primary
        };
        
        playButton.Click += (s, e) => 
        {
            if (playButton.Icon == playIcon)
            {
                mediaElement.Play();
                playButton.Icon = pauseIcon;
            }
            else
            {
                mediaElement.Pause();
                playButton.Icon = playIcon;
            }
        };

        grid.Children.Add(playButton);
        
        mediaElement.Play(); 
        return grid;
    }
}
