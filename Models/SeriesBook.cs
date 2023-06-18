namespace Audiobooks.Models
{
    public class SeriesBook : Entity
    {
        public int SeriesId { get; set; }
        public Series Series { get; set; }
        public int AudiobookId { get; set; }
        public Audiobook Audiobook { get; set; }
        public decimal SeriesNumber { get; set; }
    }
}
