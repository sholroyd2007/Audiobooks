using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Models
{
    public class Blurb
    {
        public int Id { get; set; }

        public string? AuthorName { get; set; }

        public string? BookSeries { get; set; }

        public string Description { get; set; }
    }
}
