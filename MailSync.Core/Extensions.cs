using MailKit.Net.Imap;
using MailKit;

namespace MailSync.Core;

public static class Extensions
{
    public static async Task<IMailFolder?> TryGetFolder(this ImapClient client, string folderName)
    {
        try
        {
            return await client.GetFolderAsync(folderName);
        }
        catch
        {
            return null;
        }
    }
}