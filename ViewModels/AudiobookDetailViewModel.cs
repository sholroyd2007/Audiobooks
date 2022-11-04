using Audiobooks.Models;
using System.Collections.Generic;

namespace Audiobooks.ViewModels
{
    public class AudiobookDetailViewModel
    {
        public IEnumerable<Audiobook> AuthorBooks { get; set; }
        public IEnumerable<Audiobook> NarratorBooks { get; set; }
        public IEnumerable<Audiobook> SeriesBooks { get; set; }
        public Recommendation Recommendation { get; set; }
        public Blurb SeriesBlurb { get; set; }
        public Blurb AuthorBlurb { get; set; }
        public Sample Sample { get; set; }
        public Audiobook Audiobook { get; set; }

    }
}
