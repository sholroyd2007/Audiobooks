using Audiobooks.Dtos;
using Audiobooks.Models;
using Audiobooks.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiobooks.APIControllers
{
    [ApiController]
    [Route("[Controller]")]
    public class AudiobooksController : ControllerBase
    {
        public AudiobooksController(IAPIService aPIService)
        {
            APIService = aPIService;
        }

        public IAPIService APIService { get; }

        [HttpGet("/audiobooks")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDto>>>> GetAllAudiobooks()
        {
            return Ok(await APIService.GetAllAudiobooks());
        }

        [HttpGet("/categories")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetCategoryDto>>>> GetAllCategories()
        {
            return Ok(await APIService.GetAllCategories());
        }

        [HttpGet("/authors")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<string>>>> GetAllAuthors()
        {
            return Ok(await APIService.GetAllAuthors());
        }

        [HttpGet("/narrators")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<string>>>> GetAllNarrators()
        {
            return Ok(await APIService.GetAllNarrators());
        }

        [HttpGet("/series")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<string>>>> GetAllSeries()
        {
            return Ok(await APIService.GetAllSeries());
        }

        [HttpGet("/audiobooks/{id}")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetAudiobookById(int id)
        {
            return Ok(await APIService.GetAudiobookById(id));
        }

        [HttpGet("/audiobooks/random")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetRandomAudiobook()
        {
            return Ok(await APIService.GetRandomAudiobook());
        }

        [HttpGet("/category/{id}/audiobooks")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDto>>>> GetAudiobooksByCategory(int id)
        {
            return Ok(await APIService.GetAudiobooksByCategory(id));
        }

        [HttpGet("/author/{author}/audiobooks")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDto>>>> GetAudiobooksByAuthor(string author)
        {
            return Ok(await APIService.GetAudiobooksByAuthor(author));
        }

        [HttpGet("/narrator/{narrator}/audiobooks")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDto>>>> GetAudiobooksByNarrator(string narrator)
        {
            return Ok(await APIService.GetAudiobooksByNarrator(narrator));
        }

        [HttpGet("/series/{series}/audiobooks")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDto>>>> GetAudiobooksBySeries(string series)
        {
            return Ok(await APIService.GetAudiobooksBySeries(series));
        }


    }
}
