using MailSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MailSync.Core;

public class MailContext : DbContext
{
    public string DbPath { get; } = "mail.db";

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Folder> Folders { get; set; } = null!;

    public MailContext()
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={this.DbPath}");
}