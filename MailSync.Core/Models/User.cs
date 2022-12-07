namespace MailSync.Core.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public List<Account> Accounts { get; set; } = new();
}