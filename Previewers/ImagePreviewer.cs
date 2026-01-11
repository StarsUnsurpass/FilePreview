using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FilePreview.Previewers;

public class ImagePreviewer : IPreviewer
{
    private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".ico", ".webp", ".tiff", ".tif" };

    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return Extensions.Contains(ext);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var image = new System.Windows.Controls.Image
        {
            Stretch = System.Windows.Media.Stretch.Uniform,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                var bitmap = new BitmapImage();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Essential for cross-thread access
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    image.Source = bitmap;
                });
            }
            catch (Exception)
            {
                // Handle error or leave empty
            }
        });

        return image;
    }
}
