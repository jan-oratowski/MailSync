using MailSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MailSync.Core.Services;

public class SyncService
{
    private readonly MailContext _context;
    private ConnectionService? _source, _destination;
    private int _sourceId, _destinationId;
    private bool _sourceSyncEnabled;
    public bool SyncEnabled => _sourceSyncEnabled && _destination != null;
    public SyncService(MailContext context)
    {
        _context = context;
    }

    public async Task SetDestination(int destId)
    {
        _destinationId = destId;
        _destination = new ConnectionService();
        var dc = await _context.Accounts.FirstAsync(s => s.Id == destId);
        _destination.SetConnection(dc.Server, dc.Port, dc.Login, dc.Secret, dc.UseSsl);
    }

    public async Task SetSource(int sourceId)
    {
        _sourceId = sourceId;
        _source = new ConnectionService();
        var sc = await _context.Accounts.FirstAsync(s => s.Id == sourceId);
        _source.SetConnection(sc.Server, sc.Port, sc.Login, sc.Secret, sc.UseSsl);
        _sourceSyncEnabled = sc.ShouldSync;
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

    public async Task SyncMessages(int passNo, bool force = false)
    {
        if (!force && !SyncEnabled)
            return;

        var account = await _context.Accounts
            .Include(a => a.Folders)
            .ThenInclude(f => f.Rules)
            .FirstAsync(a => a.Id == _sourceId);

        var folders =
            _context.Folders.Where(f => f.Account.Id == _sourceId)
                .Include(f => f.Rules)
                .ThenInclude(r => r.Target);

        if (!folders.Any())
            return;

        foreach (var folder in folders)
        {
            var index = 0;
            while (index > -1)
            {
                index = await _source!.GetMessageForMove(folder.Path, passNo * 30);
                if (index > -1)
                {
                    await MoveMessage(index, folder.Path, folder.MapTo!.Path);
                }
            }
        }
    }

    public async Task MoveMessage(int id, string sourcePath, string destPath)
    {
        var msg = await _source!.GetMessage(id, sourcePath);
        if (msg == null)
        {
            Console.WriteLine($"Message was null: {id}");
            return;
        }
        Console.WriteLine($"Moving message: {msg.Subject}");
        var store = await _destination!.SaveMessage(msg, destPath);
        if (store)
            await _source.DeleteMessage(id, sourcePath);
    }

}