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
        public async Task<IActionResult> Browse(int? page, string sort = null)
        {
            var audiobooks = await AudiobookService.GetSortedBooks("Browse", sort);
            if (audiobooks == null)
            {
                return NotFound();
            }
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

        public async Task<IActionResult> BookSeries(string name, int? page, string sort = null)
        {
            var audiobooks = await AudiobookService.GetSortedBooks("Series", sort, null, null, name);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("BookSeries", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Author(string author, int? page, string sort = null)
        {
            var audiobooks = await AudiobookService.GetSortedBooks("Author", sort, author);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Author", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Narrator(string narrator, int? page, string sort = null)
        {
            var audiobooks = await AudiobookService.GetSortedBooks("Narrator", sort, null, narrator);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Narrator", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Category(int? Id, int? page, string sort = null)
        {

            var audiobooks = await AudiobookService.GetSortedBooks("Category", sort, null, null, null, Id);
            if (audiobooks == null)
            {
                return NotFound();
            }
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
        public async Task<IActionResult> Search(string SearchTerm, int? page)
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                return RedirectToAction(nameof(Browse));
            }
            ViewBag.SearchTerm = SearchTerm;
            

            var results = await AudiobookService.GetSortedBooks("Search", null, null, null, null, null, SearchTerm);
            ViewBag.Count = results.Count();
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = results.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Search", onePageOfAudiobooks);

            //return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> Search(int? page, string sort = null, string search = null)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return RedirectToAction(nameof(Browse));
            }
            ViewBag.SearchTerm = search;

            var results = await AudiobookService.GetSortedBooks("Search", sort, null, null, null, null, search);
            ViewBag.Count = results.Count();
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = results.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Search", onePageOfAudiobooks);

            //return View(results);
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
        public async Task<IActionResult> RandomAudiobook()
        {
            var randomId = await AudiobookService.GetRandomBookId();
            return RedirectToAction(nameof(Detail), new { id = randomId });
        }

    }
}
