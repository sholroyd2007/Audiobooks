using Audiobooks.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Audiobooks.Dtos
{
    public class GetAudiobookDetailDto
    {
        public GetAudiobookDetailDto()
        {
            Authors = new List<GetAuthorDto>();
            Narrators = new List<GetNarratorDto>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public GetCategoryDto Category { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Length { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public List<GetAuthorDto> Authors { get; set; }
        public List<GetNarratorDto> Narrators { get; set; }
        public GetSeriesBookDto? SeriesBook { get; set; }
    }
}
