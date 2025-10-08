using System;

namespace DirectoryAnalyzer.Models;

public class FileSystemItem
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Attributes { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public int Depth { get; set; }
}
