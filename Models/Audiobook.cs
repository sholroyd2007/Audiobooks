using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Models
{
    public class Audiobook
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
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public int? FileCount { get; set; }
        public string Description { get; set; }
        public string? Series { get; set; }
        public decimal? SeriesNumber { get; set; }
    }

    public class AudiobookClassMap : ClassMap<Audiobook>
    {
        public AudiobookClassMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.CategoryId).Name("CategoryId");
            Map(m => m.Author).Name("Author");
            Map(m => m.Narrator).Name("Narrator");
            Map(m => m.Url).Name("Url");
            Map(m => m.ImageUrl).Name("ImageUrl");
            Map(m => m.Length).Name("Length");
            Map(m => m.DateAdded).Name("DateAdded");
            Map(m => m.Description).Name("Description");
            Map(m => m.Series).Name("Series");
            Map(m => m.SeriesNumber).Name("SeriesNumber");
        }
    }
}
