namespace Audiobooks.Models
{
    public class BookNarrator : Entity
    {
        public int NarratorId { get; set; }
        public Narrator Narrator { get; set; }
        public int AudiobookId { get; set; }
        public Audiobook Audiobook { get; set; }
    }
}
