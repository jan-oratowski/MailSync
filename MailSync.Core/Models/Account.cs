namespace MailSync.Core.Models;

public class Account
{
    public int Id { get; set; }
    public bool ShouldSync { get; set; }
    public string Name { get; set; } = null!;
    public string Server { get; set; } = null!;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string Login { get; set; } = null!;
    public string Secret { get; set; } = null!;
    public List<Folder> Folders { get; set; } = new();
    public User User { get; set; } = null!;
}