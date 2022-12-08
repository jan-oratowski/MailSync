namespace MailSync.Core.Models
{
    public class FolderRule
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;
        public List<Folder> Folders { get; set; } = new();
        public Folder? Target { get; set; }
        
        public int OlderThan { get; set; }
        public string? FromAddress { get; set; }
        public RuleAction Action { get; set; }
        public bool Enabled { get; set; }
    }

    public enum RuleAction
    {
        None = 0,
        Copy = 1,
        Move = 2,
        Delete = 3,
    }
}
