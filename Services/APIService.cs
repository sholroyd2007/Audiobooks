using Audiobooks.Dtos;
using Audiobooks.Models;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Audiobooks.Services
{
    public interface IAPIService
    {
        Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAllAudiobooks();
        Task<ServiceResponse<GetAudiobookDetailDto>> GetAudiobookById(int id);
        Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksByCategory(int id);
        Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksByAuthor(int authorId);
        Task<ServiceResponse<GetAudiobookDetailDto>> GetRandomAudiobook();
        Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetSearchResults(string SearchTerm);
        Task<ServiceResponse<IEnumerable<GetCategoryDto>>> GetAllCategories();
        Task<ServiceResponse<IEnumerable<GetAuthorDto>>> GetAllAuthors();
        Task<ServiceResponse<IEnumerable<GetNarratorDto>>> GetAllNarrators();
        Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksByNarrator(int narratorId);
        Task<ServiceResponse<IEnumerable<GetSeriesDto>>> GetAllSeries();
        Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksBySeries(int seriesId);
        Task<ServiceResponse<GetSeriesDto>> GetSeriesById(int id);
        Task<ServiceResponse<GetAuthorDto>> GetAuthorById(int id);
        Task<ServiceResponse<GetNarratorDto>> GetNarratorById(int id);
        Task<ServiceResponse<IEnumerable<GetAuthorDto>>> GetAuthorsByAudiobookId(int id);
        Task<ServiceResponse<IEnumerable<GetNarratorDto>>> GetNarratorsByAudiobookId(int id);
    }
    public class APIService : IAPIService
    {
        public APIService(IAudiobookService audiobookService,
            IMapper mapper,
            IAuthorService authorService)
        {
            AudiobookService = audiobookService;
            Mapper = mapper;
            AuthorService = authorService;
        }

        public IAudiobookService AudiobookService { get; }
        public IMapper Mapper { get; }
        public IAuthorService AuthorService { get; }

        private async Task<GetAudiobookDetailDto> AddBookExtras(GetAudiobookDetailDto dto)
        {
            var authors = await AudiobookService.GetAuthorsByBookId(dto.Id);
            var narrators = await AudiobookService.GetNarratorsByBookId(dto.Id);
            var seriesBook = await AudiobookService.GetSeriesBookByBookId(dto.Id);
            dto.Narrators = narrators.Select(e => Mapper.Map<GetNarratorDto>(e)).ToList();
            dto.Authors = authors.Select(e => Mapper.Map<GetAuthorDto>(e)).ToList();
            if(seriesBook != null)
            {
                dto.SeriesBook = Mapper.Map<GetSeriesBookDto>(seriesBook);
            }

            return dto;
        }
        public async Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAllAudiobooks()
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDetailDto>>();
            var books = await AudiobookService.GetAllAudiobooks();
            if (books == null)
            {
                response.Success = false;
                response.Message = "Books Not Found";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }
            
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDetailDto>(e)).ToList();
            foreach (var bookDto in response.Data)
            {
                await AddBookExtras(bookDto);
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<GetAudiobookDetailDto>> GetAudiobookById(int id)
        {
            var response = new ServiceResponse<GetAudiobookDetailDto>();
            if (id == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var book = await AudiobookService.GetAudiobookById(id);
            if (book == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Book Not Found";
                return response;
            }
            var bookDto = Mapper.Map<GetAudiobookDetailDto>(book);
            await AddBookExtras(bookDto);
            response.Data = bookDto;
            response.StatusCode = HttpStatusCode.OK;
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
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Book Not Found";
                return response;
            }

            var bookDto = Mapper.Map<GetAudiobookDetailDto>(book);
            await AddBookExtras(bookDto);
            response.Data = bookDto;
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetSearchResults(string SearchTerm)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDetailDto>>();

            var results = await AudiobookService.GetSearchResults(SearchTerm);
            if (results == null)
            {
                response.Success = false;
                response.Message = "Search Could Not Be Completed";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }
            response.Data = results.Select(e => Mapper.Map<GetAudiobookDetailDto>(e)).ToList();
            foreach (var bookDto in response.Data)
            {
                await AddBookExtras(bookDto);
            }
            response.StatusCode = HttpStatusCode.OK;
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
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }
            response.Data = categories.Select(e => Mapper.Map<GetCategoryDto>(e)).ToList();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksByCategory(int id)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDetailDto>>();
            if (id == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var books = await AudiobookService.GetBooksByCategoryId(id);
            if (books == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDetailDto>(e)).ToList();
            foreach (var bookDto in response.Data)
            {
                await AddBookExtras(bookDto);
            }
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksByAuthor(int authorId)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDetailDto>>();
            if (authorId == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var books = await AudiobookService.GetBooksByAuthor(authorId);
            if (books == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDetailDto>(e)).ToList();
            foreach (var bookDto in response.Data)
            {
                await AddBookExtras(bookDto);
            }
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAuthorDto>>> GetAllAuthors()
        {
            var response = new ServiceResponse<IEnumerable<GetAuthorDto>>();
            var authors = await AudiobookService.GetAuthors();
            if (authors == null)
            {
                response.Success = false;   
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Authors Not Found";
                return response;
            }
            response.Data = authors.Select(e=>Mapper.Map<GetAuthorDto>(e)).ToList();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetNarratorDto>>> GetAllNarrators()
        {
            var response = new ServiceResponse<IEnumerable<GetNarratorDto>>();
            var narrators = await AudiobookService.GetNarrators();
            if (narrators == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Narrators Not Found";
                return response;
            }
            response.Data = narrators.Select(e=>Mapper.Map<GetNarratorDto>(e)).ToList();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksByNarrator(int narratorId)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDetailDto>>();
            if (narratorId == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var books = await AudiobookService.GetBooksByNarrator(narratorId);
            if (books == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDetailDto>(e)).ToList();
            foreach (var bookDto in response.Data)
            {
                await AddBookExtras(bookDto);
            }
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetSeriesDto>>> GetAllSeries()
        {
            var response = new ServiceResponse<IEnumerable<GetSeriesDto>>();
            var series = await AudiobookService.GetBookSeries();
            if (series == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Series Not Found";
                return response;
            }
            response.Data = series.Select(e=> Mapper.Map<GetSeriesDto>(e)).ToList();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>> GetAudiobooksBySeries(int seriesId)
        {
            var response = new ServiceResponse<IEnumerable<GetAudiobookDetailDto>>();
            if (seriesId == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var books = await AudiobookService.GetBooksBySeries(seriesId);
            if (books == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Books Not Found";
                return response;
            }
            response.Data = books.Select(e => Mapper.Map<GetAudiobookDetailDto>(e)).ToList();
            foreach (var bookDto in response.Data)
            {
                await AddBookExtras(bookDto);
            }
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<GetSeriesDto>> GetSeriesById(int id)
        {
            var response = new ServiceResponse<GetSeriesDto>();
            if (id == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var series = await AudiobookService.GetSeriesById(id);
            if (series == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Series Not Found";
                return response;
            }
            response.Data = Mapper.Map<GetSeriesDto>(series);
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<GetAuthorDto>> GetAuthorById(int id)
        {
            var response = new ServiceResponse<GetAuthorDto>();
            if (id == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var author = await AudiobookService.GetAuthorById(id);
            if (author == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Author Not Found";
                return response;
            }
            response.Data = Mapper.Map<GetAuthorDto>(author);
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<GetNarratorDto>> GetNarratorById(int id)
        {
            var response = new ServiceResponse<GetNarratorDto>();
            if (id == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var narrator = await AudiobookService.GetNarratorById(id);
            if (narrator == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Narrator Not Found";
                return response;
            }
            response.Data = Mapper.Map<GetNarratorDto>(narrator);
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetAuthorDto>>> GetAuthorsByAudiobookId(int id)
        {
            var response = new ServiceResponse<IEnumerable<GetAuthorDto>>();
            if (id == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var authors = await AudiobookService.GetAuthorsByBookId(id);
            if (authors == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Authors Not Found";
                return response;
            }
            response.Data = authors.Select(e=>Mapper.Map<GetAuthorDto>(e)).ToList();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<GetNarratorDto>>> GetNarratorsByAudiobookId(int id)
        {
            var response = new ServiceResponse<IEnumerable<GetNarratorDto>>();
            if (id == 0)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Id is required";

                return response;

            }
            var narrators = await AudiobookService.GetNarratorsByBookId(id);
            if (narrators == null)
            {
                response.Success = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Narrators Not Found";
                return response;
            }
            response.Data = narrators.Select(e=>Mapper.Map<GetNarratorDto>(e));
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}
