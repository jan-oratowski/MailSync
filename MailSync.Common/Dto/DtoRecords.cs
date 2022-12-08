namespace MailSync.Common.Dto;

public record Account(int? Id, string Name, string Server, int Port, string Login, string Secret);

public record Folder(int Id, int AccountId, List<int> RuleIds, string Name, bool IsHidden);

public record FolderRule(int Id, List<int> FolderIds, string? Name);