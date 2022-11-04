using Audiobooks.Data;
using Audiobooks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using System.IO;
using System;
using Audiobooks.Services;
using Audiobooks.ViewModels;

namespace Audiobooks.Controllers
{

    public class HomeController : Controller

    {
        private readonly ApplicationDbContext _context;
        

        public ISiteMapService SiteMapService { get; }
        public IAudiobookService AudiobookService { get; }

        public HomeController(ApplicationDbContext context,
            ISiteMapService siteMapService,
            IAudiobookService audiobookService)
        {
            _context = context;
            SiteMapService = siteMapService;
            AudiobookService = audiobookService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Browse(int? page)
        {
            var audiobooks = await AudiobookService.GetAllAudiobooks();
            audiobooks = audiobooks.OrderByDescending(a => a.DateAdded);

            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Browse", onePageOfAudiobooks);

        }

        public async Task<IActionResult> Detail(int id)
        {
            var audiobook = await AudiobookService.GetAudiobookById(id);

            if (audiobook != null)
            {
                return View(await AudiobookService.GetDetailPageViewModel(audiobook.Id));
            }
            return NotFound();

        }

        public async Task<IActionResult> BookSeries(string name, int? page)
        {
            var audiobooks = await AudiobookService.GetBooksBySeries(name);
            if (audiobooks == null)
            {
                return NotFound();
            }

            audiobooks = audiobooks.OrderBy(e => e.SeriesNumber);
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("BookSeries", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Author(string author, int? page)
        {
            var audiobooks = await AudiobookService.GetBooksByAuthor(author);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Author", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Narrator(string narrator, int? page)
        {
            var audiobooks = await AudiobookService.GetBooksByNarrator(narrator);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Narrator", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Category(int? Id, int? page)
        {

            var audiobooks = await AudiobookService.GetBooksByCategoryId(Id.Value);

            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Category", onePageOfAudiobooks);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Search(string SearchTerm)
        {

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                return RedirectToAction(nameof(Browse));
            }

            var results = await AudiobookService.GetSearchResults(SearchTerm);

            return View(results);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }


        public async Task<IActionResult> Recommendations(int? page)
        {
            var recommendations = await AudiobookService.GetRecommendations();
            if (recommendations.Any())
            {
                var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
                var onePageOfAudiobooks = recommendations.ToPagedList(pageNumber, 5); // will only contain 25 products max because of the pageSize

                return View("Recommendations", onePageOfAudiobooks);
            }
            else
            {
                return View(recommendations);
            }
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {
            var sitemapXml = await SiteMapService.GetSitemapXml();
            return File(Encoding.UTF8.GetBytes(sitemapXml), "text/xml");
        }

        [HttpGet]
        public IActionResult RandomAudiobook()
        {
            var randomId = AudiobookService.GetRandomBookId();
            return RedirectToAction(nameof(Detail), new { id = randomId });
        }

    }
}
