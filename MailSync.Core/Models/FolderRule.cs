namespace MailSync.Core.Models
{
    public class FolderRule
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;
        public List<Folder> Folders { get; set; } = new();
        public Folder? Target { get; set; }
    }
}
