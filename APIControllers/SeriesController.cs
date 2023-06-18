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
    public class SeriesController : Controller
    {
        public SeriesController(IAPIService aPIService)
        {
            APIService = aPIService;
        }

        public IAPIService APIService { get; }


        [HttpGet("/series")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetSeriesDto>>>> GetAllSeries()
        {
            var response = await APIService.GetAllSeries();
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("/series/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetSeriesDto>>>> GetSeriesById(int id)
        {
            var response = await APIService.GetSeriesById(id);
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

        [HttpGet("/series/{seriesId}/audiobooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetAudiobooksBySeries(int seriesId)
        {
            var response = await APIService.GetAudiobooksBySeries(seriesId);
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
