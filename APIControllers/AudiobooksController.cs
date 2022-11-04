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

        [HttpGet("/api/All")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDto>>>> GetAllAudiobooks()
        {
            return Ok(await APIService.GetAllAudiobooks());
        }

        [HttpGet("/api/{id}")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetAudiobookById(int id)
        {
            return Ok(await APIService.GetAudiobookById(id));
        }

        [HttpGet("/api/Random")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetRandomAudiobook()
        {
            return Ok(await APIService.GetRandomAudiobook());
        }
    }
}
