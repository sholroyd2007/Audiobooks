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
using System.Web;
using Audiobooks.Helpers;

namespace Audiobooks.Controllers
{

    public class HomeController : Controller

    {
        private readonly ApplicationDbContext _context;


        public ISiteMapService SiteMapService { get; }
        public IAudiobookService AudiobookService { get; }
        public ISlugService SlugService { get; }
        public ISeriesService SeriesService { get; }
        public IAuthorService AuthorService { get; }
        public INarratorService NarratorService { get; }

        public HomeController(ApplicationDbContext context,
            ISiteMapService siteMapService,
            IAudiobookService audiobookService,
            ISlugService slugService,
            ISeriesService seriesService,
            IAuthorService authorService,
            INarratorService narratorService)
        {
            _context = context;
            SiteMapService = siteMapService;
            AudiobookService = audiobookService;
            SlugService = slugService;
            SeriesService = seriesService;
            AuthorService = authorService;
            NarratorService = narratorService;
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

        public async Task<IActionResult> Detail(string id)
        {
            Audiobook audiobook;
            int audiobookId;
            var inputIsId = int.TryParse(id, out audiobookId);

            if (!inputIsId)
            {
                if (id.ContainsCharactersThatNeedEncoding())
                {
                    id = HttpUtility.UrlEncode(id);
                    id = SlugHelper.Slugify(id);
                }


                audiobookId = await SlugService.GetEntityIdBySlug<Audiobook>(id);
            }


            audiobook = await AudiobookService.GetAudiobookById(audiobookId);
            var authors = await _context.BookAuthors.AsNoTracking().Include(e => e.Author).Where(e => e.AudiobookId == audiobook.Id).Select(e => e.Author).ToListAsync();
            var narrators = await _context.BookNarrators.AsNoTracking().Include(e => e.Narrator).Where(e => e.AudiobookId == audiobook.Id).Select(e => e.Narrator).ToListAsync();
            var vm = new AudiobookDetailViewModel();
            var authorBooks = new List<Audiobook>();
            var narratorBooks = new List<Audiobook>();
            Series series = null;
            IEnumerable<Audiobook> seriesBooks = null;
            var seriesBookDefault = await _context.SeriesBooks.AsNoTracking().FirstOrDefaultAsync(e => e.AudiobookId == audiobook.Id);
            if(seriesBookDefault != null)
            {
                series = await _context.Series.AsNoTracking().FirstOrDefaultAsync(e => e.Id == seriesBookDefault.SeriesId);
                seriesBooks = await AudiobookService.GetBooksInSeries(audiobook.Id);
            }

            foreach (var author in authors)
            {
                var books = _context.BookAuthors.AsNoTracking().Include(e => e.Audiobook).Where(e => e.AuthorId == author.Id).Select(e=>e.Audiobook).ToList();
                authorBooks.AddRange(books);
            }

            foreach (var narrator in narrators)
            {
                var books = _context.BookNarrators.AsNoTracking().Include(e => e.Audiobook).Where(e => e.NarratorId == narrator.Id).Select(e => e.Audiobook).ToList();
                narratorBooks.AddRange(books);
            }

            authorBooks = authorBooks.DistinctBy(e=>e.Id).ToList();
            narratorBooks = narratorBooks.DistinctBy(e=>e.Id).ToList();

            vm.Audiobook = audiobook;
            vm.SeriesBooks = seriesBooks;
            vm.Recommendation = await AudiobookService.GetRecommendationByBookId(audiobook.Id);
            vm.Authors = authors;
            vm.Narrators = narrators;
            vm.AuthorBooks = authorBooks;
            vm.NarratorBooks = narratorBooks;
            vm.Series = series;
            vm.CurrentSeriesBook = seriesBookDefault;

            if (audiobook == null)
            {
                return NotFound();
            }
            return View(vm);

        }

        public async Task<IActionResult> BookSeries(string id, int? page, string sort = null)
        {
            Series series;
            int seriesId;
            var inputIsId = int.TryParse(id, out seriesId);

            if (!inputIsId)
            {
                if (id.ContainsCharactersThatNeedEncoding())
                {
                    id = HttpUtility.UrlEncode(id);
                    id = SlugHelper.Slugify(id);
                }


                seriesId = await SlugService.GetEntityIdBySlug<Series>(id);
            }


            series = await SeriesService.GetSeriesById(seriesId);

            ViewBag.SeriesId = series.Id;
            var audiobooks = await AudiobookService.GetSortedBooks("Series", sort, null, null, series);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("BookSeries", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Author(string id, int? page, string sort = null)
        {
            Author author;
            int authorId;
            var inputIsId = int.TryParse(id, out authorId);

            if (!inputIsId)
            {
                if (id.ContainsCharactersThatNeedEncoding())
                {
                    id = HttpUtility.UrlEncode(id);
                    id = SlugHelper.Slugify(id);
                }


                authorId = await SlugService.GetEntityIdBySlug<Author>(id);
            }


            author = await AuthorService.GetAuthorById(authorId);

            ViewBag.authId = author.Id;
            var audiobooks = await AudiobookService.GetSortedBooks("Author", sort, author);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Author", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Narrator(string id, int? page, string sort = null)
        {
            Narrator narrator;
            int narratorId;
            var inputIsId = int.TryParse(id, out narratorId);

            if (!inputIsId)
            {
                if (id.ContainsCharactersThatNeedEncoding())
                {
                    id = HttpUtility.UrlEncode(id);
                    id = SlugHelper.Slugify(id);
                }


                narratorId = await SlugService.GetEntityIdBySlug<Narrator>(id);
            }


            narrator = await NarratorService.GetNarratorById(narratorId);

            ViewBag.NarratorId = narrator.Id;
            var audiobooks = await AudiobookService.GetSortedBooks("Narrator", sort, null, narrator);
            if (audiobooks == null)
            {
                return NotFound();
            }
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfAudiobooks = audiobooks.ToPagedList(pageNumber, 9); // will only contain 25 products max because of the pageSize

            return View("Narrator", onePageOfAudiobooks);
        }

        public async Task<IActionResult> Category(string id, int? page, string sort = null)
        {

            Category category;
            int categoryId;
            var inputIsId = int.TryParse(id, out categoryId);

            if (!inputIsId)
            {
                if (id.ContainsCharactersThatNeedEncoding())
                {
                    id = HttpUtility.UrlEncode(id);
                    id = SlugHelper.Slugify(id);
                }


                categoryId = await SlugService.GetEntityIdBySlug<Category>(id);
            }


            category = await AudiobookService.GetCategoryById(categoryId);

            var audiobooks = await AudiobookService.GetSortedBooks("Category", sort, null, null, null, category.Id);
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
            var audiobook = await AudiobookService.GetAudiobookById(randomId);
            var slug = await SlugService.GetSlugForEntity(audiobook);
            return RedirectToAction(nameof(Detail), new { id = slug.Name });
        }

    }
}
