using Audiobooks.Models;
using System;

namespace Audiobooks.Dtos
{
    public class GetAudiobookDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public string Authors { get; set; }
        public string Narrators { get; set; }

    }
}
