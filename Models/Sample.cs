using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Models
{
    public class Sample
    {
        public int Id { get; set; }

        public Audiobook Audiobook { get; set; }

        public int AudiobookId { get; set; }

        public byte[] Data { get; set; }

        public string ContentType { get; set; }
    }
}
