using MailKit;
using MailSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MailSync.Core.Services;

public class SyncService
{
    private readonly MailContext _context;
    private ConnectionService? _source, _destination;
    private int _sourceId, _destinationId;
    public SyncService(MailContext context)
    {
        _context = context;
    }

    public async Task SetDestination(int destId)
    {
        _destinationId = destId;
        _destination = new ConnectionService();
        var dc = await _context.Accounts.FirstAsync(s => s.Id == destId);
        _destination.SetConnection(dc.Server, dc.Port, dc.User, dc.Secret, dc.UseSsl);
    }

    public async Task SetSource(int sourceId)
    {
        _sourceId = sourceId;
        _source = new ConnectionService();
        var sc = await _context.Accounts.FirstAsync(s => s.Id == sourceId);
        _source.SetConnection(sc.Server, sc.Port, sc.User, sc.Secret, sc.UseSsl);
    }

    public async Task DownloadDirectories()
    {
        var dirs = await _source!.GetAllDirectories();

        var account = await _context.Accounts
            .Include(a => a.Folders)
            .FirstAsync(a => a.Id == _sourceId);

        foreach (var dir in dirs)
        {
            if (account.Folders.Any(d => d.Path == dir))
                continue;

            account.Folders.Add(new Folder
            {
                Path = dir,
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task SyncMessages(int passNo)
    {
        var account = await _context.Accounts
            .Include(a => a.Folders).FirstAsync(a => a.Id == _sourceId);

        var folders = account.
            Folders.Where(f => f.Pass == passNo && f.MapTo?.Account.Id == _destinationId);

        foreach (var folder in folders)
        {
            var folderMessages = await _source!.ListMessages(folder.Path, passNo * 180);
            foreach (var message in folderMessages)
            {
                await MoveMessage(message, folder.Path, folder.MapTo!.Path);
            }
        }
    }

    public async Task MoveMessage(UniqueId uid, string sourcePath, string destPath)
    {
        var msg = await _source!.GetMessage(uid, sourcePath);
        if (msg == null)
        {
            Console.WriteLine($"Message was null: {uid}");
            return;
        }
        var store = await _destination!.SaveMessage(msg, destPath);
        if (store)
            await _source.DeleteMessage(uid, sourcePath);
    }

}