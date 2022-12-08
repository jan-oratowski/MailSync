namespace MailSync.Core.Models;

public class Folder
{
    public int Id { get; set; }
    public string Path { get; set; } = null!;
    public bool IsHidden { get; set; }
    public Account Account { get; set; } = null!;
    public List<FolderRule> Rules { get; set; } = new();
}