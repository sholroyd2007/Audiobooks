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
    public class CategoriesController : Controller
    {
        public CategoriesController(IAPIService aPIService)
        {
            APIService = aPIService;
        }

        public IAPIService APIService { get; }


        [HttpGet("/categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResponseCache(Duration = 600)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetCategoryDto>>>> GetAllCategories()
        {
            var response = await APIService.GetAllCategories();
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("/category/{id}/audiobooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetAudiobookDetailDto>>>> GetAudiobooksByCategory(int id)
        {
            var response = await APIService.GetAudiobooksByCategory(id);
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
