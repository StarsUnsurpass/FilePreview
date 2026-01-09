using System.Collections.Generic;
using System.Linq;

namespace FilePreview.Previewers;

public class PreviewerFactory
{
    private readonly List<Lazy<IPreviewer>> _previewers = new()
    {
        new Lazy<IPreviewer>(() => new FolderPreviewer()),
        new Lazy<IPreviewer>(() => new MarkdownPreviewer()),
        new Lazy<IPreviewer>(() => new WebPreviewer()),
        new Lazy<IPreviewer>(() => new CsvPreviewer()),
        new Lazy<IPreviewer>(() => new ZipPreviewer()),
        new Lazy<IPreviewer>(() => new FontPreviewer()),
        new Lazy<IPreviewer>(() => new CertificatePreviewer()),
        new Lazy<IPreviewer>(() => new TextPreviewer()),
        new Lazy<IPreviewer>(() => new ImagePreviewer()),
        new Lazy<IPreviewer>(() => new MediaPreviewer()),
        new Lazy<IPreviewer>(() => new HexPreviewer()),
        new Lazy<IPreviewer>(() => new ShellPreviewer())
    };

    public IPreviewer? GetPreviewer(string filePath)
    {
        return _previewers.Select(p => p.Value).FirstOrDefault(p => p.CanPreview(filePath));
    }
}
