using Audiobooks.Dtos;
using Audiobooks.Models;
using Audiobooks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetAllAudiobooks()
        {
            var response = await APIService.GetAllAudiobooks();
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("/audiobooks/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetAudiobookById(int id)
        {
            
            var response = await APIService.GetAudiobookById(id);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(response);
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("/audiobooks/random")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetRandomAudiobook()
        {
            var response = await APIService.GetRandomAudiobook();
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

    }
}
