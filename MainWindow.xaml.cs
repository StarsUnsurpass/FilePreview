using System;
using System.Windows;
using FilePreview.Previewers;
using Wpf.Ui.Controls;

namespace FilePreview;

public partial class MainWindow : FluentWindow
{
    private readonly PreviewerFactory _previewerFactory = new();
    private string? _currentFilePath;

    public MainWindow()
    {
        InitializeComponent();
        
        try
        {
            var iconUri = new Uri("pack://application:,,,/app.ico");
            this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
        }
        catch { /* Fallback to default icon */ }

        // InfoBadge still shows on hover
        this.MouseEnter += (s, e) => InfoBadge.Visibility = Visibility.Visible;
        this.MouseLeave += (s, e) => InfoBadge.Visibility = Visibility.Collapsed;
        InfoBadge.Visibility = Visibility.Collapsed;
    }

    private void CopyPath_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            System.Windows.Clipboard.SetText(_currentFilePath);
        }
    }

    private void StayOnTop_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem menuItem)
        {
            this.Topmost = menuItem.IsChecked;
        }
    }

    private void ShowDetails_Click(object sender, RoutedEventArgs e)
    {
        // Toggle additional info if needed
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{_currentFilePath}\"");
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            ShowPreview(_currentFilePath);
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private bool _isShowingDialog = false;

    private void About_Click(object sender, RoutedEventArgs e)
    {
        _isShowingDialog = true;
        string aboutText = "FilePreview v1.0\n\n" +
                           "一款 Windows 平台下的轻量级文件预览工具，旨在提供类似 macOS Quick Look 的极速体验。\n\n" +
                           "作者: StarsUnsurpass\n" +
                           "项目主页: github.com/StarsUnsurpass/FilePreview\n\n" +
                           "目前支持的格式:\n" +
                           "• 图像: JPG, PNG, BMP, GIF, WEBP, ICO\n" +
                           "• 文本与代码: TXT, MD, JSON, XML, CS, PY, CPP, HTML, CSS 等\n" +
                           "• 专业文档: PDF, Markdown\n" +
                           "• 多媒体: MP4, MKV, AVI, MP3, WAV, FLAC\n" +
                           "• 压缩包: ZIP (查看内部结构)\n" +
                           "• 其他: 文件夹概览, Office 文档 (利用系统句柄)";

        System.Windows.MessageBox.Show(this, aboutText, "关于 FilePreview", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        _isShowingDialog = false;
    }

    private void GitHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/StarsUnsurpass/FilePreview",
                UseShellExecute = true
            });
        }
        catch { }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HidePreview();
    }

    public void HidePreview()
    {
        if (_isShowingDialog) return;
        this.Hide();
        PreviewContent.Content = null;
        _currentFilePath = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape || e.Key == System.Windows.Input.Key.Space)
        {
            HidePreview();
            e.Handled = true;
        }
    }

    private void RootBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void MainWindow_Deactivated(object sender, EventArgs e)
    {
        // Only hide if we are NOT minimized and NOT showing a dialog
        if (this.WindowState != WindowState.Minimized && !_isShowingDialog)
        {
            HidePreview();
        }
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _currentFilePath,
                    UseShellExecute = true
                });
                HidePreview();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Could not open file: {ex.Message}");
            }
        }
    }

    public void ShowPreview(string filePath)
    {
        _currentFilePath = filePath;
        TitleTextBlock.Text = System.IO.Path.GetFileName(filePath);
        FormatInfoText.Text = System.IO.Path.GetExtension(filePath).ToUpper().TrimStart('.');

        UpdateFileIcon(filePath);

        var previewer = _previewerFactory.GetPreviewer(filePath);
        FrameworkElement content;

        if (previewer != null)
        {
            content = previewer.CreateControl(filePath);
        }
        else
        {
            content = new System.Windows.Controls.TextBlock
            {
                Text = "No preview available for this file type.",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Gray
            };
        }

        PreviewContent.Content = content;
        AdjustWindowSize(content, filePath);

        this.Show();
        this.Activate();
        this.Focus();
    }

    private void UpdateFileIcon(string filePath)
    {
        var ext = System.IO.Path.GetExtension(filePath).ToLower();
        FileIcon.Symbol = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" => SymbolRegular.Image24,
            ".zip" or ".rar" or ".7z" => SymbolRegular.Archive24,
            ".mp4" or ".mkv" or ".avi" => SymbolRegular.Video24,
            _ => System.IO.Directory.Exists(filePath) ? SymbolRegular.Folder24 : SymbolRegular.Document24
        };
    }

    private void AdjustWindowSize(FrameworkElement content, string filePath)
    {
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        double targetWidth = 800;
        double targetHeight = 600;

        if (content is System.Windows.Controls.Image image && image.Source is System.Windows.Media.Imaging.BitmapSource bitmap)
        {
            targetWidth = bitmap.PixelWidth;
            targetHeight = bitmap.PixelHeight;

            double ratio = Math.Min(screenWidth * 0.8 / targetWidth, screenHeight * 0.8 / targetHeight);
            if (ratio < 1)
            {
                targetWidth *= ratio;
                targetHeight *= ratio;
            }
        }
        else if (content is System.Windows.Controls.StackPanel) // Folder
        {
            targetWidth = 600;
            targetHeight = 500;
        }

        this.Width = Math.Max(400, targetWidth);
        this.Height = Math.Max(300, targetHeight + 50);

        // Center on screen
        System.Drawing.Point mousePos;
        Windows.Win32.PInvoke.GetCursorPos(out mousePos);

        this.Left = (screenWidth - this.Width) / 2;
        this.Top = (screenHeight - this.Height) / 2;
    }
}