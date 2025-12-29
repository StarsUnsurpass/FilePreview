using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FilePreview.Previewers;

public class MediaPreviewer : IPreviewer
{
    private static readonly string[] VideoExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".wmv" };
    private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".flac", ".ogg", ".wma", ".m4a" };

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

        // Basic controls
        var playButton = new System.Windows.Controls.Button 
        { 
            Content = "Play", 
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center, 
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0,0,0,10)
        };
        
        playButton.Click += (s, e) => 
        {
            if (playButton.Content.ToString() == "Play")
            {
                mediaElement.Play();
                playButton.Content = "Pause";
            }
            else
            {
                mediaElement.Pause();
                playButton.Content = "Play";
            }
        };

        grid.Children.Add(playButton);
        
        mediaElement.Play(); // Auto play
        playButton.Content = "Pause";

        return grid;
    }
}
