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
        ".asm", ".s", ".cmake", ".editorconfig", ".env", ".gitignore", ".dockerignore"
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
            Foreground = System.Windows.Media.Brushes.White // Simple dark theme assumption
        };

        var ext = Path.GetExtension(filePath).ToLower();
        textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(ext);
        
        // Load content
        textEditor.Text = File.ReadAllText(filePath);

        return textEditor;
    }
}
