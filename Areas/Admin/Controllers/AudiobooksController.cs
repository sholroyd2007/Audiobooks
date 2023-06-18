using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Audiobooks.Data;
using Audiobooks.Models;
using Microsoft.AspNetCore.Authorization;
using Audiobooks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AudiobooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IAudiobookService AudiobookService { get; }
        public IAuthorService AuthorService { get; }
        public INarratorService NarratorService { get; }
        public ISeriesService SeriesService { get; }
        public ISlugService SlugService { get; }

        public AudiobooksController(ApplicationDbContext context,
            IAudiobookService audiobookService,
            IAuthorService authorService,
            INarratorService narratorService,
            ISeriesService seriesService,
            ISlugService slugService)
        {
            _context = context;
            AudiobookService = audiobookService;
            AuthorService = authorService;
            NarratorService = narratorService;
            SeriesService = seriesService;
            SlugService = slugService;
        }

        // GET: Audiobooks

        public async Task<IActionResult> Index()
        {
            var audiobooks = await AudiobookService.GetAllAudiobooks();
            return View(audiobooks);
        }

        // GET: Audiobooks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                var audiobook = await AudiobookService.GetAudiobookById(id.Value);
                if (audiobook != null)
                {
                    return View(audiobook);
                }
                return NotFound();
            }
            return NotFound();

        }

        // GET: Audiobooks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Audiobooks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Audiobook audiobook, [FromForm] string authors, [FromForm] string narrators, [FromForm] string series, [FromForm] decimal seriesNumber)
        {
            if (ModelState.IsValid)
            {

                var newAudiobook = await AudiobookService.AddAudiobook(audiobook);
                //Add Authors
                string[] auths = authors.Split(',');
                foreach (var item in auths)
                {
                    var author = await _context.Authors.AsNoTracking().FirstOrDefaultAsync(e => e.Name.ToLower() == item.ToLower());
                    if (author == null)
                    {
                        author = new Author();
                        author.Name = item.Trim();
                        await AuthorService.AddAuthor(author);
                    }
                    var bookAuthor = await _context.BookAuthors.AsNoTracking().FirstOrDefaultAsync(e => e.AudiobookId == audiobook.Id && e.AuthorId == author.Id);
                    if (bookAuthor == null)
                    {
                        bookAuthor = new BookAuthor()
                        {
                            AudiobookId = audiobook.Id,
                            AuthorId = author.Id
                        };
                        await AuthorService.AddBookAuthor(bookAuthor);
                    }

                }

                //Add Narrators
                string[] narrs = narrators.Split(',');
                foreach (var item in narrs)
                {
                    var narrator = await _context.Narrators.AsNoTracking().FirstOrDefaultAsync(e => e.Name.ToLower() == item.ToLower());
                    if (narrator == null)
                    {
                        narrator = new Narrator();
                        narrator.Name = item.Trim();
                        await NarratorService.AddNarrator(narrator);
                    }
                    var bookNarrator = await _context.BookNarrators.AsNoTracking().FirstOrDefaultAsync(e => e.AudiobookId == audiobook.Id && e.NarratorId == narrator.Id);
                    if (bookNarrator == null)
                    {
                        bookNarrator = new BookNarrator()
                        {
                            AudiobookId = audiobook.Id,
                            NarratorId = narrator.Id
                        };
                        await NarratorService.AddBookNarrator(bookNarrator);
                    }

                }

                //Add Series and SeriesBook
                if (series != null)
                {
                    var _series = await _context.Series.AsNoTracking().FirstOrDefaultAsync(e => e.Name == series);
                    if (_series == null)
                    {
                        _series = new Series();
                        _series.Name = series.Trim();
                        await SeriesService.AddSeries(_series);
                    }

                    var seriesBook = new SeriesBook();
                    seriesBook.SeriesId = _series.Id;
                    seriesBook.AudiobookId = audiobook.Id;
                    seriesBook.SeriesNumber = seriesNumber;

                    await SeriesService.AddSeriesBook(seriesBook);
                }

                await SlugService.GenerateSlugs();

                return RedirectToAction(nameof(Index));
            }
            return View(audiobook);
        }

        // GET: Audiobooks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var audiobook = await AudiobookService.GetAudiobookById(id.Value);
            if (audiobook != null)
            {
                return View(audiobook);
            }
            return NotFound();

        }



        // POST: Audiobooks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Audiobook audiobook, [FromForm] string authors, [FromForm] string narrators, [FromForm] string series, [FromForm] decimal seriesNumber)
        {
            if (id != audiobook.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AudiobookService.EditAudiobook(id, audiobook);
                    
                    //Add Authors
                    if (authors != null)
                    {
                        string[] auths = authors.Split(',');
                        foreach (var item in auths)
                        {
                            var author = await _context.Authors.AsNoTracking().FirstOrDefaultAsync(e => e.Name.ToLower() == item.ToLower());
                            if (author == null)
                            {
                                author = new Author();
                                author.Name = item.Trim();
                                await AuthorService.AddAuthor(author);
                            }
                            var bookAuthor = await _context.BookAuthors.AsNoTracking().FirstOrDefaultAsync(e => e.AudiobookId == audiobook.Id && e.AuthorId == author.Id);
                            if (bookAuthor == null)
                            {
                                bookAuthor = new BookAuthor()
                                {
                                    AudiobookId = audiobook.Id,
                                    AuthorId = author.Id
                                };
                                await AuthorService.AddBookAuthor(bookAuthor);
                            }

                        }
                    }

                    //Add Narrators
                    if (narrators != null)
                    {
                        string[] narrs = narrators.Split(',');
                        foreach (var item in narrs)
                        {
                            var narrator = await _context.Narrators.AsNoTracking().FirstOrDefaultAsync(e => e.Name.ToLower() == item.ToLower());
                            if (narrator == null)
                            {
                                narrator = new Narrator();
                                narrator.Name = item.Trim();
                                await NarratorService.AddNarrator(narrator);
                            }
                            var bookNarrator = await _context.BookNarrators.AsNoTracking().FirstOrDefaultAsync(e => e.AudiobookId == audiobook.Id && e.NarratorId == narrator.Id);
                            if (bookNarrator == null)
                            {
                                bookNarrator = new BookNarrator()
                                {
                                    AudiobookId = audiobook.Id,
                                    NarratorId = narrator.Id
                                };
                                await NarratorService.AddBookNarrator(bookNarrator);
                            }

                        }
                    }

                    //Add Series and SeriesBook
                    if (series != null)
                    {
                        var _series = await _context.Series.AsNoTracking().FirstOrDefaultAsync(e => e.Name == series);
                        if (_series == null)
                        {
                            _series = new Series();
                            _series.Name = series.Trim();
                            await SeriesService.AddSeries(_series);

                            
                        }
                        var seriesBook = new SeriesBook();
                        seriesBook.SeriesId = _series.Id;
                        seriesBook.AudiobookId = audiobook.Id;
                        seriesBook.SeriesNumber = seriesNumber;

                        await SeriesService.AddSeriesBook(seriesBook);
                        
                    }

                    //Update Slugs
                    await SlugService.GenerateSlugs();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AudiobookExists(audiobook.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return View(audiobook);
            }
            return View(audiobook);
        }

        // GET: Audiobooks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var audiobook = await AudiobookService.GetAudiobookById(id.Value);
            if (audiobook != null)
            {
                return View(audiobook);

            }
            return NotFound();

        }

        // POST: Audiobooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await AudiobookService.DeleteAudiobook(id);
            return RedirectToAction(nameof(Index));
        }

        private bool AudiobookExists(int id)
        {
            return _context.Audiobook.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ImportCatalogue(IFormFile file, [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            if (file != null)
            {
                await AudiobookService.ImportCatalogue(file, hostingEnvironment);
            }
            return RedirectToAction(nameof(Index), new { Controller = "Audiobooks", Area = "Admin" });
        }

        public async Task<IActionResult> AddAuthorsAndNarrators(IFormFile file, [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            if (file != null)
            {
                await AudiobookService.UploadAuthorsAndNarrators(file, hostingEnvironment);
            }
            return RedirectToAction(nameof(Index), new { Controller = "Audiobooks", Area = "Admin" });
        }

        public async Task<IActionResult> DeleteItems()
        {
            var authors = await _context.Authors.ToListAsync();
            _context.RemoveRange(authors);
            await _context.SaveChangesAsync();

            var authorBooks = await _context.BookAuthors.ToListAsync();
            _context.RemoveRange(authorBooks);
            await _context.SaveChangesAsync();


            var narrators = await _context.Narrators.ToListAsync();
            _context.RemoveRange(narrators);
            await _context.SaveChangesAsync();

            var narratorBooks = await _context.BookNarrators.ToListAsync();
            _context.RemoveRange(narratorBooks);
            await _context.SaveChangesAsync();

            var series = await _context.Series.ToListAsync();
            _context.RemoveRange(series);
            await _context.SaveChangesAsync();

            var seriesBooks = await _context.SeriesBooks.ToListAsync();
            _context.RemoveRange(seriesBooks);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { Controller = "Audiobooks", Area = "Admin" });
        }

        public async Task<IActionResult> DeleteAllBooks(int id)
        {
            await AudiobookService.DeleteAllAudiobooks();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteBookAuthor(int id)
        {
            var audiobookId = (await _context.BookAuthors.AsNoTracking().Include(e => e.Audiobook).FirstOrDefaultAsync(e => e.Id == id)).AudiobookId;
            await AuthorService.DeleteBookAuthor(id);
            return RedirectToAction(nameof(Edit), new { id = audiobookId });
        }

        public async Task<IActionResult> DeleteBookNarrator(int id)
        {
            var audiobookId = (await _context.BookNarrators.AsNoTracking().Include(e => e.Audiobook).FirstOrDefaultAsync(e => e.Id == id)).AudiobookId;
            await NarratorService.DeleteBookNarrator(id);
            return RedirectToAction(nameof(Edit), new { id = audiobookId });
        }

        public async Task<IActionResult> DeleteSeriesBook(int id)
        {
            var audiobookId = (await _context.SeriesBooks.AsNoTracking().Include(e => e.Audiobook).FirstOrDefaultAsync(e => e.Id == id)).AudiobookId;
            await SeriesService.DeleteSeriesBook(id);
            return RedirectToAction(nameof(Edit), new { id = audiobookId });
        }
    }
}
