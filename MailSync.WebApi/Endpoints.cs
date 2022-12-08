using Microsoft.AspNetCore.Mvc;

namespace MailSync.WebApi;

using Core;
using Core.Models;
using Microsoft.AspNetCore.Builder;

public static class Endpoints
{
    public static void RegisterAccountEndpoints(this WebApplication app)
    {
        const int fakeUserId = 1;

        app.MapGet("/accounts", (MailContext context) => 
        {
            return context.Accounts
                .Where(a => a.User.Id == fakeUserId)
                .Select(a => new Common.Dto.Account(a.Id, a.Name, a.Server, a.Port, a.Login, a.Secret))
                .ToList();
        });

        app.MapPost("/accounts", (MailContext context, Common.Dto.Account account) =>
        {

            var user = context.Users.First(u => u.Id == fakeUserId);
            if (account.Id.HasValue)
            {
                var fromDb = user.Accounts.FirstOrDefault(a => a.Id == account.Id);
                if (fromDb == null)
                    return false;

                fromDb.Name = account.Name;
                fromDb.Server = account.Server;
                fromDb.Port = account.Port;
                fromDb.Login = account.Login;
                fromDb.Secret = account.Secret;
            }
            else
            {
                user.Accounts.Add(new Account
                {
                    Name = account.Name,
                    Login = account.Login,
                    Secret = account.Secret,
                    Port = account.Port,
                    Server = account.Server,
                });
            }
                
            context.SaveChanges();

            return true;
        });

        app.MapDelete("/accounts", (MailContext context, int accountId) =>
        {
            var account = context.Accounts.FirstOrDefault(a => a.User.Id == fakeUserId && a.Id == accountId);
            if (account == null)
                return false;

            context.Accounts.Remove(account);
            context.SaveChanges();
            return true;
        });
    }

    public static void RegisterFolderEndpoints(this WebApplication app)
    {
        const int fakeUserId = 1;

        app.MapGet("/account/{accountId}/folders", (MailContext context, [FromQuery] int accountId) =>
        {
            var folders = context.Folders
                .Where(f => f.Account.Id == accountId && f.Account.User.Id == fakeUserId);
            
            return !folders.Any() ?
                new List<Common.Dto.Folder>() :
                folders.Select(f => new Common.Dto.Folder(f.Id, accountId, f.Rules.Select(r => r.Id).ToList(), f.Path, f.IsHidden)).ToList();
        });

        app.MapPost("/folders", (MailContext context, Common.Dto.Folder folder) =>
        {
            var folderInDb = context.Folders
                .FirstOrDefault(f => f.Account.User.Id == fakeUserId);

            if (folderInDb == null)
                return false;

            folderInDb.IsHidden = folder.IsHidden;
            folderInDb.Path = folder.Name;

            context.SaveChanges();
            return true;
        });


    }
}