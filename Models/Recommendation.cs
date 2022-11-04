using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Models
{
    public class Recommendation
    {
        public int Id { get; set; }

        public Audiobook audiobook { get; set; }

        public int AudiobookId { get; set; }

        public string Description { get; set; }
    }
}
