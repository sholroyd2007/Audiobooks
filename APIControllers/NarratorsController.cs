using Audiobooks.Dtos;
using Audiobooks.Models;
using Audiobooks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Audiobooks.APIControllers
{
    [ApiController]
    [Route("[Controller]")]
    public class NarratorsController : Controller
    {
        public NarratorsController(IAPIService aPIService)
        {
            APIService = aPIService;
        }

        public IAPIService APIService { get; }


        [HttpGet("/narrators")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<string>>>> GetAllNarrators()
        {
            var response = await APIService.GetAllNarrators();
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("/narrators/{narratorId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<string>>>> GetNarratorById(int narratorId)
        {
            var response = await APIService.GetNarratorById(narratorId);
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

        [HttpGet("/narrator/{narratorId}/audiobooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetAudiobooksByNarrator(int narratorId)
        {
            var response = await APIService.GetAudiobooksByNarrator(narratorId);
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
    }
}
