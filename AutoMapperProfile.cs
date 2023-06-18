using Audiobooks.Dtos;
using Audiobooks.Models;
using AutoMapper;

namespace Audiobooks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Audiobook, GetAudiobookDto>().ReverseMap();
            CreateMap<Audiobook, GetAudiobookDetailDto>().ReverseMap();
            CreateMap<Category, GetCategoryDto>().ReverseMap();
            CreateMap<Author, GetAuthorDto>().ReverseMap();
            CreateMap<Narrator, GetNarratorDto>().ReverseMap();
            CreateMap<Series, GetSeriesDto>().ReverseMap();
            CreateMap<SeriesBook, GetSeriesBookDto>().ReverseMap();
        }
    }
}
