using MailSync.Core;
using MailSync.Core.Services;
using Microsoft.EntityFrameworkCore;

MailContext? ctx = null;

if (args.Length > 0)
{
    var connString = args[0];
    ctx = new MailContext(connString);
}

ctx ??= new MailContext();

Console.WriteLine("Migrations starting.");
ctx.Database.Migrate();
await ctx.SaveChangesAsync();
Console.WriteLine("Migrations done.");

var syncServices = new List<SyncService>();

foreach (var account in ctx.Accounts)
{
    Console.WriteLine($"Downloading folders for {account.Name} started.");
    var source = new SyncService(ctx);
    await source.SetSource(account.Id);
    await source.DownloadDirectories();
    syncServices.Add(source);
    Console.WriteLine($"Downloading folders for {account.Name} done.");
}

foreach (var sync in syncServices)
{
    await sync.SetDestination(2);
    for (var i = 0; i <= 48; i++)
    {
        await sync.SyncMessages(i);
    }
}



