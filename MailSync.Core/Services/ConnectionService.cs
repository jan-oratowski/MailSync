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

    public async Task<List<UniqueId>> ListMessages(string dirPath, int olderThanDays)
    {
        try
        {
            await EnsureConnected();

            var dir = await _client!.GetFolderAsync(dirPath);
            _ = await dir.OpenAsync(FolderAccess.ReadOnly);

            var allDates = await dir.FetchAsync(0, -1, MessageSummaryItems.InternalDate);

            var filteredDates = allDates
                .Where(m => m.Date < DateTime.UtcNow.AddDays(olderThanDays * -1)).OrderBy(m => m.Date);

            return filteredDates.Select(d => d.UniqueId).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<UniqueId>();
        }
    }

    public async Task<MimeMessage?> GetMessage(UniqueId uid, string dirPath)
    {
        try
        {
            await EnsureConnected();

            var dir = await _client!.GetFolderAsync(dirPath);
            _ = await dir.OpenAsync(FolderAccess.ReadOnly);
            return await dir.GetMessageAsync(uid);
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

            var uid = await dir.AppendAsync(FormatOptions.Default, msg);
            return uid != null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteMessage(UniqueId uid, string dirPath)
    {
        try
        {
            await EnsureConnected();

            var dir = await _client!.GetFolderAsync(dirPath);
            _ = await dir.OpenAsync(FolderAccess.ReadWrite);

            await dir.StoreAsync(uid, new StoreFlagsRequest(StoreAction.Add, MessageFlags.Deleted) { Silent = true });
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