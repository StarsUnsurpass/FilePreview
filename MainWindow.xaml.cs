using System;
using System.Windows;
using FilePreview.Previewers;
using Serilog;
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
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load window icon.");
        }

        // InfoBadge still shows on hover
        this.MouseEnter += (s, e) => InfoBadge.Visibility = Visibility.Visible;
        this.MouseLeave += (s, e) => InfoBadge.Visibility = Visibility.Collapsed;
        InfoBadge.Visibility = Visibility.Collapsed;
    }

    private void CopyPath_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            try
            {
                System.Windows.Clipboard.SetText(_currentFilePath);
                Log.Debug("Copied path to clipboard: {FilePath}", _currentFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to copy path to clipboard.");
            }
        }
    }

    private void CopyContent_Click(object sender, RoutedEventArgs e)
    {
        string? textToCopy = null;

        if (PreviewContent.Content is ICSharpCode.AvalonEdit.TextEditor textEditor)
        {
            textToCopy = textEditor.Text;
        }
        else if (PreviewContent.Content is System.Windows.Controls.TextBlock textBlock)
        {
            textToCopy = textBlock.Text;
        }
        else if (PreviewContent.Content is System.Windows.Controls.ScrollViewer scrollViewer && scrollViewer.Content is System.Windows.Controls.StackPanel stackPanel)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var child in stackPanel.Children)
            {
                if (child is System.Windows.Controls.TextBlock tb) sb.AppendLine(tb.Text);
            }
            textToCopy = sb.ToString();
        }

        if (!string.IsNullOrEmpty(textToCopy))
        {
            try
            {
                System.Windows.Clipboard.SetText(textToCopy);
                Log.Debug("Copied preview content to clipboard.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to copy content to clipboard.");
            }
        }
    }

    private void StayOnTop_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem menuItem)
        {
            this.Topmost = menuItem.IsChecked;
            Log.Debug("Toggled StayOnTop: {Topmost}", this.Topmost);
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
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{_currentFilePath}\"");
                Log.Debug("Opened folder for file: {FilePath}", _currentFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open folder.");
            }
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            Log.Debug("Refreshing preview for: {FilePath}", _currentFilePath);
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
        string aboutText = "FilePreview v1.6.0\n\n" +
                           "一款 Windows 平台下的轻量级文件预览工具，旨在提供类似 macOS Quick Look 的极速体验。\n\n" +
                           "作者: StarsUnsurpass\n" +
                           "项目主页: github.com/StarsUnsurpass/FilePreview\n\n" +
                           "目前支持的格式:\n" +
                           "• 图像: JPG, PNG, BMP, GIF, WEBP, ICO, TIFF, SVG\n" +
                           "• 3D 模型: STL, OBJ, 3DS, PLY\n" +
                           "• 文本与代码: 几乎所有主流编程语言及配置文件\n" +
                           "• 专业文档: PDF, Markdown\n" +
                           "• 字体: TTF, OTF, WOFF, WOFF2\n" +
                           "• 证书: CER, CRT, DER, PEM\n" +
                           "• 多媒体: MP4, MKV, AVI, WEBM, MP3, WAV, FLAC, AAC\n" +
                           "• 压缩包: ZIP, EPUB, JAR, APK, NUPKG, VSIX (查看结构)\n" +
                           "• 二进制: DLL, EXE, BIN (十六进制视图)\n" +
                           "• 其他: 文件夹概览 (带统计信息), Office 文档 (利用系统句柄)";

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
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open GitHub link.");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HidePreview();
    }

    public void HidePreview()
    {
        if (_isShowingDialog) return;
        Log.Debug("Hiding preview window.");
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
                Log.Information("Opening file externally: {FilePath}", _currentFilePath);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _currentFilePath,
                    UseShellExecute = true
                });
                HidePreview();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open file: {FilePath}", _currentFilePath);
                System.Windows.MessageBox.Show($"Could not open file: {ex.Message}");
            }
        }
    }

    public async void ShowPreview(string filePath)
    {
        try
        {
            Log.Information("Starting preview for: {FilePath}", filePath);
            _currentFilePath = filePath;
            TitleTextBlock.Text = System.IO.Path.GetFileName(filePath);
            FormatInfoText.Text = System.IO.Path.GetExtension(filePath).ToUpper().TrimStart('.');

            UpdateFileIcon(filePath);

            // Show loading state
            LoadingOverlay.Visibility = Visibility.Visible;
            PreviewContent.Opacity = 0;
            
            // Show window immediately so user sees something happening
            this.Show();
            this.Activate();
            this.Focus();

            // Run preview generation on background thread if possible, or just defer UI creation
            // Note: UI controls must be created on UI thread, but we can delay slightly to allow window to render
            await System.Threading.Tasks.Task.Delay(10); 

            var previewer = _previewerFactory.GetPreviewer(filePath);
            FrameworkElement content;

            if (previewer != null)
            {
                Log.Debug("Using previewer: {PreviewerType}", previewer.GetType().Name);
                content = previewer.CreateControl(filePath);
            }
            else
            {
                Log.Warning("No previewer found for file: {FilePath}", filePath);
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
            
            LoadingOverlay.Visibility = Visibility.Collapsed;
            
            // Play Fade In Animation
            if (TryFindResource("FadeInStoryboard") is System.Windows.Media.Animation.Storyboard sb)
            {
                sb.Begin();
            }
            else
            {
                PreviewContent.Opacity = 1;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error showing preview for file: {FilePath}", filePath);
            System.Windows.MessageBox.Show($"Error showing preview: {ex.Message}");
            LoadingOverlay.Visibility = Visibility.Collapsed;
            PreviewContent.Opacity = 1;
        }
    }

    private void UpdateFileIcon(string filePath)
    {
        var ext = System.IO.Path.GetExtension(filePath).ToLower();
        FileIcon.Symbol = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".ico" or ".webp" => SymbolRegular.Image24,
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".nupkg" or ".iso" or ".img" or ".torrent" => SymbolRegular.Archive24,
            ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" => SymbolRegular.Video24,
            ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" => SymbolRegular.MusicNote224,
            ".pdf" => SymbolRegular.DocumentPdf24,
            ".html" or ".htm" or ".js" or ".ts" or ".css" or ".tsv" => SymbolRegular.Code24,
            ".cs" or ".cpp" or ".py" or ".go" or ".rs" or ".diff" or ".patch" => SymbolRegular.CodeBlock24,
            ".ttf" or ".otf" or ".woff" => SymbolRegular.TextFont24,
            ".cer" or ".crt" or ".pfx" => SymbolRegular.Certificate24,
            ".md" or ".markdown" => SymbolRegular.DocumentText24,
            _ => System.IO.Directory.Exists(filePath) ? SymbolRegular.Folder24 : SymbolRegular.Document24
        };
    }

    private void AdjustWindowSize(FrameworkElement content, string filePath)
    {
        double targetWidth = 800;
        double targetHeight = 600;

        if (content is System.Windows.Controls.Image image && image.Source is System.Windows.Media.Imaging.BitmapSource bitmap)
        {
            targetWidth = bitmap.PixelWidth;
            targetHeight = bitmap.PixelHeight;
        }
        else if (content is System.Windows.Controls.StackPanel || (content is System.Windows.Controls.ScrollViewer sv && sv.Content is System.Windows.Controls.StackPanel))
        {
            targetWidth = 600;
            targetHeight = 500;
        }

        // Get the screen where the mouse is
        System.Drawing.Point mousePos;
        Windows.Win32.PInvoke.GetCursorPos(out mousePos);
        
        var screen = System.Windows.Forms.Screen.FromPoint(mousePos);
        var workingArea = screen.WorkingArea;

        double maxW = workingArea.Width * 0.85;
        double maxH = workingArea.Height * 0.85;

        double ratio = Math.Min(maxW / targetWidth, maxH / targetHeight);
        if (ratio < 1)
        {
            targetWidth *= ratio;
            targetHeight *= ratio;
        }

        this.Width = Math.Max(400, targetWidth);
        this.Height = Math.Max(300, targetHeight + 45); // 45 is title bar height

        // Center on mouse position but keep within working area
        this.Left = mousePos.X - this.Width / 2;
        this.Top = mousePos.Y - this.Height / 2;

        // Boundary checks
        if (this.Left < workingArea.Left) this.Left = workingArea.Left;
        if (this.Top < workingArea.Top) this.Top = workingArea.Top;
        if (this.Left + this.Width > workingArea.Right) this.Left = workingArea.Right - this.Width;
        if (this.Top + this.Height > workingArea.Bottom) this.Top = workingArea.Bottom - this.Height;
    }
}