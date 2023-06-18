using System;

namespace Audiobooks.Models
{
    public class Entity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
