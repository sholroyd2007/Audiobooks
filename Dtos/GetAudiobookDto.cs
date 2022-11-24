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
        public string  Author { get; set; }
    }
}
