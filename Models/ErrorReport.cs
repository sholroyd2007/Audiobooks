namespace Audiobooks.Models
{
    public class ErrorReport : Entity
    {
        public int? AudiobookId { get; set; }
        public Audiobook Audiobook { get; set; }
        public ErrorStatus ErrorStatus { get; set; }
    }

    public enum ErrorStatus
    {
        New,
        InProgress,
        Resolved
    }
}
