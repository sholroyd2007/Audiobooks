namespace Audiobooks.Models
{
    public class BookAuthor : Entity
    {
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public int AudiobookId { get; set; }
        public Audiobook Audiobook { get; set; }
    }
}
