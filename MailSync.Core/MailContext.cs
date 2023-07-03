using MailSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MailSync.Core;

public class MailContext : DbContext
{
    private readonly string? _connString;

    private const string DbFile = "mail.db";
    public string? DbPath { get; }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Folder> Folders { get; set; } = null!;

    public MailContext()
    {
        DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), DbFile);
    }
    
    public MailContext(string? connString)
    {
        _connString = connString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (string.IsNullOrEmpty(_connString))
            options.UseSqlite($"Data Source={DbPath}");
        else
            options.UseMySQL(_connString);
    }
}