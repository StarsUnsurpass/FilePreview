using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace FilePreview.Previewers;

public class HexPreviewer : IPreviewer
{
    private static readonly string[] Extensions = { 
        ".dll", ".exe", ".bin", ".dat", ".class", ".so", ".dylib", 
        ".o", ".obj", ".lib", ".a", ".pdb", ".suo", ".user",
        ".iso", ".img", ".dmp", ".torrent"
    };

    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return Extensions.Contains(ext);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var textEditor = new TextEditor
        {
            IsReadOnly = true,
            ShowLineNumbers = false,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 14,
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = System.Windows.Media.Brushes.White,
            WordWrap = false,
            HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            Text = "Loading hex view..."
        };

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                // Read first 16KB max for preview to keep it fast
                const int maxBytes = 16 * 1024; 
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var buffer = new byte[Math.Min(stream.Length, maxBytes)];
                stream.ReadExactly(buffer);

                string formattedHex = FormatHex(buffer, stream.Length > maxBytes);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    textEditor.Text = formattedHex;
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    textEditor.Text = $"Error reading file: {ex.Message}";
                });
            }
        });

        return textEditor;
    }

    private string FormatHex(byte[] data, bool truncated)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < data.Length; i += 16)
        {
            // Offset
            sb.Append(i.ToString("X8"));
            sb.Append("  ");

            // Hex bytes
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                    sb.Append(data[i + j].ToString("X2") + " ");
                else
                    sb.Append("   ");
                
                if (j == 7) sb.Append(" "); // Extra space after 8 bytes
            }

            sb.Append(" |");

            // ASCII representation
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                {
                    char c = (char)data[i + j];
                    sb.Append(char.IsControl(c) ? '.' : c);
                }
                else
                {
                    sb.Append(" ");
                }
            }
            sb.Append("|");
            sb.AppendLine();
        }

        if (truncated)
        {
            sb.AppendLine();
            sb.AppendLine("... (Content truncated for preview) ...");
        }

        return sb.ToString();
    }
}
