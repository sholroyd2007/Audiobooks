using Audiobooks.Dtos;
using Audiobooks.Models;
using AutoMapper;

namespace Audiobooks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Audiobook, GetAudiobookDto>();
            CreateMap<Audiobook, GetAudiobookDetailDto>();
        }
    }
}
