using System.Windows;

namespace FilePreview.Previewers;

public interface IPreviewer
{
    bool CanPreview(string filePath);
    FrameworkElement CreateControl(string filePath);
}
