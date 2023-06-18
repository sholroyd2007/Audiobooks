using System.Collections.Generic;

namespace Audiobooks.Models
{
    public class Series : Entity
    {
        public IEnumerable<SeriesBook> Books { get; set; }

        public Series()
        {
            this.Books = new List<SeriesBook>();
        }
    }
}
