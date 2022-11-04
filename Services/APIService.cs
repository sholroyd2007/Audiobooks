using Audiobooks.Dtos;
using Audiobooks.Models;
using AutoMapper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Services
{
    public interface IAPIService
    {
        Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAllAudiobooks();
        Task<ServiceResponse<GetAudiobookDetailDto>> GetAudiobookById(int id);
        Task<ServiceResponse<GetAudiobookDetailDto>> GetRandomAudiobook();
        Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetSearchResults(string SearchTerm);
    }
    public class APIService : IAPIService
    {
        public APIService(IAudiobookService audiobookService,
            IMapper mapper)
        {
            AudiobookService = audiobookService;
            Mapper = mapper;
        }

        public IAudiobookService AudiobookService { get; }
        public IMapper Mapper { get; }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAllAudiobooks()
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDto>>();
            var books = await AudiobookService.GetAllAudiobooks();
            if (books == null)
            {
                response.Success = false;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDto>(e)).ToList();
            return response;
        }

        public async Task<ServiceResponse<GetAudiobookDetailDto>> GetAudiobookById(int id)
        {
            var response = new ServiceResponse<GetAudiobookDetailDto>();
            var book = await AudiobookService.GetAudiobookById(id);
            if (book == null)
            {
                response.Success = false;
                response.Message = "Book Not Found";
                return response;
            }
            response.Data = Mapper.Map<GetAudiobookDetailDto>(book);
            return response;
        }

        public async Task<ServiceResponse<GetAudiobookDetailDto>> GetRandomAudiobook()
        {
            var response = new ServiceResponse<GetAudiobookDetailDto>();
            var randomId = AudiobookService.GetRandomBookId();
            var book = await AudiobookService.GetAudiobookById(randomId);
            if (book == null)
            {
                response.Success = false;
                response.Message = "Book Not Found";
                return response;
            }
            response.Data = Mapper.Map<GetAudiobookDetailDto>(book);
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetSearchResults(string SearchTerm)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDto>>();

            var results = await AudiobookService.GetSearchResults(SearchTerm);
            if (results == null)
            {
                response.Success = false;
                response.Message = "Search Results Not Found";
                return response;
            }
            response.Data = results.Select(e => Mapper.Map<GetAudiobookDto>(e)).ToList();
            return response;
        }
    }
}
