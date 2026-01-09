using System.IO;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace FilePreview.Previewers;

public class TextPreviewer : IPreviewer
{
    private static readonly string[] Extensions = { 
        ".txt", ".json", ".xml", ".cs", ".js", ".py", ".cpp", ".h", ".css", ".yaml", ".ini", ".log",
        ".ts", ".tsx", ".jsx", ".java", ".kt", ".gradle", ".sql", ".sh", ".bat", ".ps1", 
        ".config", ".props", ".targets", ".toml", ".dockerfile", ".yml",
        ".go", ".rs", ".rb", ".php", ".vue", ".lua", ".swift", ".dart", ".r", ".pl", ".vb", ".fs", 
        ".asm", ".s", ".cmake", ".editorconfig", ".env", ".gitignore", ".dockerignore",
        ".jsonld", ".webmanifest", ".lock", ".mdx", ".svelte", ".astro", ".mjs", ".cjs",
        ".vbs", ".bas", ".cls", ".frm", ".ctl", ".def", ".p12", ".pem", ".key",
        ".csproj", ".sln", ".user", ".resx", ".xaml", ".targets", ".props", ".manifest",
        ".babelrc", ".eslintrc", ".prettierrc", ".stylelintrc", ".browserslistrc"
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
            ShowLineNumbers = true,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 14,
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = System.Windows.Media.Brushes.White,
            Padding = new Thickness(10)
        };

        var ext = Path.GetExtension(filePath).ToLower();
        textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(ext);
        
        try
        {
            const long maxFileSize = 1 * 1024 * 1024; // 1MB
            var fileInfo = new FileInfo(filePath);
            
            if (fileInfo.Length > maxFileSize)
            {
                using (var reader = new StreamReader(filePath))
                {
                    char[] buffer = new char[1024 * 100]; // Read first 100KB
                    int read = reader.ReadBlock(buffer, 0, buffer.Length);
                    textEditor.Text = new string(buffer, 0, read) + "\r\n\r\n... (File too large, showing first 100KB) ...";
                }
            }
            else
            {
                string content = File.ReadAllText(filePath);
                if (ext == ".json")
                {
                    try
                    {
                        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
                        content = System.Text.Json.JsonSerializer.Serialize(jsonDoc, options);
                    }
                    catch { /* Keep original if parse fails */ }
                }
                textEditor.Text = content;
            }
        }
        catch (Exception ex)
        {
            textEditor.Text = $"Error loading file: {ex.Message}";
        }

        return textEditor;
    }
}
