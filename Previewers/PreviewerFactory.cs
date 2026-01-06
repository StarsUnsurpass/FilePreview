using System.Collections.Generic;
using System.Linq;

namespace FilePreview.Previewers;

public class PreviewerFactory
{
    private readonly List<IPreviewer> _previewers = new()
    {
        new FolderPreviewer(),
        new MarkdownPreviewer(),
        new WebPreviewer(),
        new CsvPreviewer(),
        new ZipPreviewer(),
        new TextPreviewer(),
        new ImagePreviewer(),
        new MediaPreviewer(),
        new HexPreviewer(),
        new ShellPreviewer()
    };

    public IPreviewer? GetPreviewer(string filePath)
    {
        return _previewers.FirstOrDefault(p => p.CanPreview(filePath));
    }
}
