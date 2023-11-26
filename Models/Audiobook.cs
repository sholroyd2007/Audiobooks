using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Models
{
    public class Audiobook : Entity
    {
        public Category Category { get; set; }
        public int CategoryId { get; set; }
        public IEnumerable<Author>? Authors{ get; set; }
        public IEnumerable<Narrator>? Narrators{ get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Length { get; set; }
        public string SampleUrl { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }
        public int Downloads { get; set; }

        public bool Error { get; set; }

        public Audiobook()
        {
            this.Authors = new List<Author>();
            this.Narrators = new List<Narrator>();
        }
    }

    public class AudiobookClassMap : ClassMap<Audiobook>
    {
        public AudiobookClassMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.CategoryId).Name("CategoryId");
            Map(m => m.Url).Name("Url");
            Map(m => m.ImageUrl).Name("ImageUrl");
            Map(m => m.Length).Name("Length");
            Map(m => m.DateAdded).Name("DateAdded");
            Map(m => m.Description).Name("Description");
            //Map(m => m.Series).Name("Series");
            //Map(m => m.SeriesNumber).Name("SeriesNumber");
        }
    }
}
