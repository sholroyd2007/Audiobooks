using Audiobooks.Models;
using System;

namespace Audiobooks.Dtos
{
    public class GetAudiobookDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
        public string Author { get; set; }
        public string Narrator { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Length { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public string? Series { get; set; }
        public decimal? SeriesNumber { get; set; }
    }
}
