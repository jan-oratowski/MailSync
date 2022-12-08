using MailSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MailSync.Core;

public class MailContext : DbContext
{
    private const string DbFile = "mail.db";
    public string DbPath { get; }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Folder> Folders { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public MailContext()
    {
        DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), DbFile);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={this.DbPath}");
}