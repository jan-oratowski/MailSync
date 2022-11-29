namespace MailSync.Core.Models;

public class Folder
{
    public int Id { get; set; }
    public string Path { get; set; } = null!;
    public Folder? MapTo { get; set; }
    public int Pass { get; set; }
    public Account Account { get; set; } = null!;
}