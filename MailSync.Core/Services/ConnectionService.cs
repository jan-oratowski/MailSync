// ReSharper disable PossibleMultipleEnumeration

using MailKit;
using MailKit.Net.Imap;
using MimeKit;

namespace MailSync.Core.Services;

internal class ConnectionService
{
    private string? _server, _login, _secret;
    private int? _port;
    private bool _useSsl;
    private ImapClient? _client;

    public void SetConnection(string server, int port, string login, string secret, bool useSsl)
    {
        _server = server;
        _login = login;
        _secret = secret;
        _port = port;
        _useSsl = useSsl;
    }

    private async Task EnsureConnected()
    {
        if (_client is { IsConnected: true })
            return;

        _client = await Connect();
    }

    private Task<ImapClient> Connect()
    {
        if (_server != null && _login != null && _secret != null)
            return Connect(_server, _port ?? 995, _login, _secret, _useSsl);

        throw new Exception("Connection parameters not set!");
    }

    private static async Task<ImapClient> Connect(string server, int port, string login, string secret, bool useSsl)
    {
        var client = new ImapClient();
        await client.ConnectAsync(server, port, useSsl);
        await client.AuthenticateAsync(login, secret);
        return client;
    }

    public async Task<List<string>> GetAllDirectories()
    {
        try
        {
            await EnsureConnected();

            var personal = _client!.GetFolder(_client.PersonalNamespaces[0]);
            var dirs = await Dir(personal);

            return dirs;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new();
        }
    }

    public async Task<int> GetMessageForMove(string dirPath, int olderThanDays)
    {
        try
        {
            await EnsureConnected();

            var dir = await _client!.GetFolderAsync(dirPath);
            _ = await dir.OpenAsync(FolderAccess.ReadOnly);

            var allDates = await dir.FetchAsync(0, 2, MessageSummaryItems.All);

            var filteredDates = allDates
                .Where(m => m.Date < DateTime.UtcNow.AddDays(olderThanDays * -1)).OrderBy(m => m.Date);

            if (!filteredDates.Any())
            {
                allDates = await dir.FetchAsync(0, -1, MessageSummaryItems.All);

                filteredDates = allDates
                    .Where(m => m.Date < DateTime.UtcNow.AddDays(olderThanDays * -1)).OrderBy(m => m.Date);
            }

            if (!filteredDates.Any())
                return -1;

            return filteredDates.First().Index;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return -1;
        }
    }

    public async Task<MimeMessage?> GetMessage(int idx, string dirPath)
    {
        try
        {
            await EnsureConnected();

            var dir = await _client!.GetFolderAsync(dirPath);
            _ = await dir.OpenAsync(FolderAccess.ReadOnly);
            return await dir.GetMessageAsync(idx);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<bool> SaveMessage(MimeMessage msg, string dirPath)
    {
        try
        {
            await EnsureConnected();

            var dir = await _client!.GetFolderAsync(dirPath);
            _ = await dir.OpenAsync(FolderAccess.ReadWrite);
            _ = await dir.AppendAsync(FormatOptions.Default, msg);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteMessage(int idx, string dirPath)
    {
        try
        {
            await EnsureConnected();

            var dir = await _client!.GetFolderAsync(dirPath);
            _ = await dir.OpenAsync(FolderAccess.ReadWrite);

            if (_server!.Contains("gmail"))
            {
                var trash =
                    await _client!.GetFolderAsync("[Gmail]/Trash") ??
                    await _client!.GetFolderAsync("[Google Mail]/Bin") ??
                    await _client!.GetFolderAsync("[Gmail]/Bin");
                await dir.MoveToAsync(idx, trash);
                return true;
            }

            await dir.StoreAsync(idx, new StoreFlagsRequest(StoreAction.Add, MessageFlags.Deleted) { Silent = true });
            await dir.ExpungeAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private static async Task<List<string>> Dir(IMailFolder folder)
    {
        var list = new List<string>
        {
            folder.FullName,
        };

        try
        {
            foreach (var mailFolder in await folder.GetSubfoldersAsync(false))
                list.AddRange(await Dir(mailFolder));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return list;
    }

    
}