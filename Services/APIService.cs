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
        Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksByCategory(int id);
        Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksByAuthor(string author);
        Task<ServiceResponse<GetAudiobookDetailDto>> GetRandomAudiobook();
        Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetSearchResults(string SearchTerm);
        Task<ServiceResponse<IEnumerable<GetCategoryDto>>> GetAllCategories();
        Task<ServiceResponse<IEnumerable<string>>> GetAllAuthors();
        Task<ServiceResponse<IEnumerable<string>>> GetAllNarrators();
        Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksByNarrator(string narrator);
        Task<ServiceResponse<IEnumerable<string>>> GetAllSeries();
        Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksBySeries(string series);
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
            var randomId = await AudiobookService.GetRandomBookId();
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

        public async Task<ServiceResponse<IEnumerable<GetCategoryDto>>> GetAllCategories()
        {
            var response = new ServiceResponse<IEnumerable<GetCategoryDto>>();
            var categories = await AudiobookService.GetCategories();
            if (categories == null)
            {
                response.Success = false;
                response.Message = "Categories Not Found";
                return response;
            }
            response.Data = categories.Select(e => Mapper.Map<GetCategoryDto>(e)).ToList();
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksByCategory(int id)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDto>>();
            var books = await AudiobookService.GetBooksByCategoryId(id);
            if (books == null)
            {
                response.Success = false;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDto>(e)).ToList();
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksByAuthor(string author)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDto>>();
            var books = await AudiobookService.GetBooksByAuthor(author);
            if (books == null)
            {
                response.Success = false;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDto>(e)).ToList();
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<string>>> GetAllAuthors()
        {
            var response = new ServiceResponse<IEnumerable<string>>();
            var authors = await AudiobookService.GetAuthors();
            if (authors == null)
            {
                response.Success = false;
                response.Message = "Authors Not Found";
                return response;
            }
            response.Data = authors;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<string>>> GetAllNarrators()
        {
            var response = new ServiceResponse<IEnumerable<string>>();
            var narrators = await AudiobookService.GetNarrators();
            if (narrators == null)
            {
                response.Success = false;
                response.Message = "Narrators Not Found";
                return response;
            }
            response.Data = narrators;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksByNarrator(string narrator)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDto>>();
            var books = await AudiobookService.GetBooksByNarrator(narrator);
            if (books == null)
            {
                response.Success = false;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDto>(e)).ToList();
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<string>>> GetAllSeries()
        {
            var response = new ServiceResponse<IEnumerable<string>>();
            var series = await AudiobookService.GetBookSeries();
            if (series == null)
            {
                response.Success = false;
                response.Message = "Series Not Found";
                return response;
            }
            response.Data = series.Where(e=>e != null   );
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDto>>> GetAudiobooksBySeries(string series)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDto>>();
            var books = await AudiobookService.GetBooksBySeries(series);
            if (books == null)
            {
                response.Success = false;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDto>(e)).ToList();
            return response;
        }
    }
}
