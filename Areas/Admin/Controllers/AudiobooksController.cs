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
using DocumentFormat.OpenXml.InkML;

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

        public async Task<IActionResult> Errors()
        {
            var audiobooks = await AudiobookService.GetAllAudiobooks();
            var results = audiobooks.Where(e => e.Error);
            return View(results);
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

        public async Task<IActionResult> DeleteAllErrorReports()
        {
            var reports = await AudiobookService.GetAllErrorReports();
            foreach (var item in reports)
            {
                _context.Entry(item.Audiobook).State = EntityState.Detached;
                _context.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> CheckAudiobookErrors()
        {
            string[] bookArray = {
                "https://mega.nz/file/s6sXSCJR#g_v_Hgsbi5jEg0H5K9U1lR_vL7P54HjfNBmNPz-oeEc",
"https://mega.nz/file/0ulXBTLD#IehBJR5yFVNYYfDzoUxZhZbJ6mCQRa7gFWeCsD53GYU",
"https://mega.nz/file/0nNHHLrY#KN1cciLwAKPwjfMeo0irVmohfyvitVnoKyP2kQWuep4",
"https://mega.nz/file/lmNCVQDJ#Z2OK-MKqyHXRl4q3aeijtn9FVrjSfvD35zFyoK13Qe4",
"https://mega.nz/file/cr01nBzI#FJNTq67cCCmpK-jl1EdbX51NOYb12QRSWi1yxzNxgnc",
"https://mega.nz/file/0jtRGaBS#iYeGwxdR1Nw3ng6pqiGyJ9AdZZyv2t4YoUhBqGBmNjU",
"https://mega.nz/file/kz8yxSqB#bg0OUQC-XV7_NM9BWRafjEeiPxdPk4DczQ-LaiwpnUo",
"https://mega.nz/file/hjVyBJYZ#TeZjVSB5NvwCEJ6pGbndS6XARvCnhMf_mWgiHy7aa-c",
"https://mega.nz/file/12lGTJgD#RP8YLG6X2ekGgiT_heWyf_mZuaz-fBg_dLaxlOI-Wpo",
"https://mega.nz/file/sq1H2CKT#PHWr00IpBhJV4SeFHUh8uRoXMmi7BaqDZB_GCv_6p2g",
"https://mega.nz/file/pqsA3RgJ#hWgawcoFCFhP9tRuLtolt-G_AiNC1_HVgKJKPGJogTQ",
"https://mega.nz/file/Yjs1BB5b#F-rv5R3gldzsiWrWi1gNMAcBGVxm_yX3QbAJCJzmv0Y",
"https://mega.nz/file/1yNV2KCA#MiApppZLasdis4u1nbtr0MVxzClepGOiSqguwZNRNGk",
"https://mega.nz/file/MzlA3QaZ#oT31OEBwFO4NmjlXqf6cLYlYjxJPsFTz7Sp87XJTR6Q",
"https://mega.nz/file/AiVSgKCT#Xqp4AEDaV4QDVdYkUCuj4qlxJ9qlPDrY5UXscLE-l50",
"https://mega.nz/file/lvlmjI4C#ZOmtase8CC4P4-mwveSEw0br8x3Ti-_4FIaO1oXyYzA",
"https://mega.nz/file/1zdyyBrZ#jwOg-mPGeefKjt9GHIoEyfwUfLiyYufAfy7nMdrUiF4",
"https://mega.nz/file/I3kAnYSK#mK6USb5anJPnpe7sN6DwdQfEiE6ABJcTRZKHPDUfI7Y",
"https://mega.nz/file/A6VG1Bgb#sX8gZRYvExCaS4cxh4K25mgi1Wsw3umEVqdiQ7XwO0g",
"https://mega.nz/file/ciFgRb5C#iIdpHnlkGqlCuoDFkBz6thevJyaYAvFbGzxMplTX45U",
"https://mega.nz/file/cut2kAYa#fXxoprNebp5c48kBCT2Uw2JHuetSI1k2_zdTHsR45JA",
"https://mega.nz/file/Zzc3zaSK#_TmEoIgc2uROvnvWWFCQfPGbR942r6EEOYkMn-N7yzc",
"https://mega.nz/file/R2kR1B4K#pgKOfKoBvkSlkDoHz2ZlRCV8MVOYsWiCGyUUXjE-1xU",
"https://mega.nz/file/Iz9nlRKK#bd9NBMOxHeSM-RNASYSeflV_8zrMnYqjnH89RzvVJlA",
"https://mega.nz/file/8nlV3JxK#Dl3CXosnssb3si0EiBGdA4qo42qo74vTgkISoSHNPkw",
"https://mega.nz/file/JvkF2ZCJ#Unt-6tfUyXqpEb2l1XmkfAzzfKo__RnLe_nVDaaDzqs",
"https://mega.nz/file/lrMElCja#A7yIBSa4MRoHY7v5M4kixvJIB-zmINSADTt2pwmWYuE",
"https://mega.nz/file/xi801bJR#WGljI-iI2xQ3hFNusoRpKR13mF91UvXwXxv5BgbybXE",
"https://mega.nz/file/UnFwRZDI#Y1PNJJPGj5Dgaseqx_LfMV7yxszXIvaan4ylorEZRzo",
"https://mega.nz/file/1jcVmLaJ#ZGi0Vhw_1h35lod_Cq5f4bBIuHwN2HH4-efN3etjbPg",
"https://mega.nz/file/x3sQwQYb#u2vmd6hXpbeCueKE08acn_d-6hAObRlAotYGjdEXOeQ",
"https://mega.nz/file/NvUD2KCI#SaOoHQMz_LMImTBDs1RjZC1myVLmZ8IInePQEy0gZjk",
"https://mega.nz/file/Nu9V2SBR#t5AqJ60F9i_BrdXHrVATET5gP8AaE9HPohkkIYuY3Tc",
"https://mega.nz/file/cv1DmagR#OdHr5Lxc7cbLG1h4Awr9Bu5_fhPdTjPs36LDsW_V1Q0",
"https://mega.nz/file/dq9mzZjJ#vFEA9Nf5368YxNZ5SGa0LT3bNNq5Oldb_aIXQVl_ubU",
"https://mega.nz/file/MzMjCbgb#2mLAh7PLwZt9FlGLqwX9q8vAFwkpeYuhpEKu91JrZA8",
"https://mega.nz/file/Jv8wDZjZ#DJl4jQ8sb-YSuy8a3vD7GgiU3E_xLcQX2e-EqlxG8Q0",
"https://mega.nz/file/dvMCUT6J#WcNZBNTROrqZGl5-1gbeqFyv_JhgkKqwKxwm9_45eBg",
"https://mega.nz/file/5ndBwYRS#cKqIpyj2Z8qxvdX5TB4RCgGYpY7ITTJvpT6pXPf7i9s",
"https://mega.nz/file/ontiFRrL#nViL0v-QMRNuR8RaIN7MAdcyjK1ouaEDLTyhjO3UdEY",
"https://mega.nz/file/J6ViCbIB#JbwQuFGsYNWuAzaKvKESULdKHSpoMvch_68ALv5C0yY",
"https://mega.nz/file/EutQkKTQ#b4Yp-TP6pmcq5mfEMXIh6t_Ehxo2ceucrGMnL6chw80",
"https://mega.nz/file/8nEVwKIL#FAYjBVVfdAMb68jS3dY0S7LPPlyJF1d0P6qjssWVWQY",
"https://mega.nz/file/ontzFDxQ#fW3DubJj_ByiZXLUWWD8Anlz-zg5bO3Uj39PI2vItf8",
"https://mega.nz/file/Qj0QGL4S#JRBIgqUTGiJNDa4Dj8ZFnxDJcXq7L2EwM4oQf2uuQAY",
"https://mega.nz/file/pj1FlYRQ#4OBJd7gCZ8KptM2VlAhCtvu7jCfglybEfT_6fN04oIE",
"https://mega.nz/file/F6tnUabL#KPfedSJlWl2Ocv7dCLMjD-nghBwywmdJIWFhPMPYw2Q",
"https://mega.nz/file/5vFUzApK#DkapwyiDOHa5NKsa7khkhP0fo85pZffP48vd6lO5yAI",
"https://mega.nz/file/Zr1QkT6Z#7ccdJRZa9knTXK9UIJ6m-s6fwYepSC-bsKjgmGLcYBE",
"https://mega.nz/file/di8lyIoB#J9yaEgqVVvmn-BKahxptXT9F351PDMy8GuI-S9H-nFU",
"https://mega.nz/file/hqVjlBpa#DXfbDP7hxVUHFTkDK5Qtmm_t-h9GK_9ndRRnTLp7Hkc",
"https://mega.nz/file/w7UDWBrI#1SHYCGrZEy8nundtPqnxkwQ6z0-HRxmm7PJc-X-Kcao",
"https://mega.nz/file/lrV0nZoB#s845-riM3Wg0cr4CG1XOHdHhHBHEni3vUSjKP8YKcUo",
"https://mega.nz/file/RrdVyKhI#V5jqpyAA9oXNQ2hwts8jgaD7VpsXsHCCBE-HQX6gXNs",
"https://mega.nz/file/BicEDZrQ#tvAdvD4zafMzgGs3Gc_oujv0g6nfBDADjZFsBokMyD8",
"https://mega.nz/file/p20jnA5B#dlQhYO1fIARqqp5DubNBBAcQpsENPzIfiW0iByEUJUU",
"https://mega.nz/file/5ndHQTLA#D_184gLh1ZM7uTR4GbMtZ4rZRhIse9RNrwMSiYzRdMc",
"https://mega.nz/file/t2MhBYwY#t1cGJdpNgp-28BKbewe6gEVkQNQBmLN7MarzyPAzgY0",
"https://mega.nz/file/QmEFlLDJ#7DTfzPUzrVsZLKG4NbKdKBcX9WbKdx22NEEKl15trFM",
"https://mega.nz/file/h2cjVQSb#GThZzyS4DXFqVFNFZGtOUF1Uxd1Id3rAPrBk-nbwXNg",
"https://mega.nz/file/9rlRnTDT#wFbG-nUsk9Rvg7JtwB4prQ9dnqD4aD-DlGp1K6FI8v8",
"https://mega.nz/file/9yMGibxD#L5dvKiXgtMFuZ0NKYqBgULX6pMNmVuXMu7xjUzddJNs",
"https://mega.nz/file/hrEVDJzD#S5RHlz2BhdScdxtB3gtCEMnPrOhOAIeQByImP-DM6zs",
"https://mega.nz/file/RuNmBCjR#zSFylo1oPYdk1QK2RDB0YY8AwRmHPnAsJ_FXTNM6MU0",
"https://mega.nz/file/B68UjYpI#lTmSWlTmvJXUoOuvZWY-imPnMG1Ohppk9fwZJ-5IWOU",
"https://mega.nz/file/suMEkTpR#15hhmdx-ee4aMTrsnbFjMGVEt0R2C0DbgjHNfqX1LOI",
"https://mega.nz/file/9q1RBYbZ#b6H580x1gXKfuu3WRF0z0_7nEXFkVO3zxjk4fd96xm0",
"https://mega.nz/file/ljlAxT6J#eqIzz3fAWCl_NoRNW-5aedCDyPjZzlsEQ7HBhKToKNI",
"https://mega.nz/file/on1jxIIa#6JEeYlZgNV8l6U_MJt2O2sCKlJd1mctkKDBIlJJssBM",
"https://mega.nz/file/xjUVBbga#roGwqzdk1Ol6PU0buA9fX6QUvuFlU86x0eJNmT0NlC0",
"https://mega.nz/file/Vvl0Ca4J#7VH1XwpNIzkafy4u_pu0NFAohrTsBkoA5VT1ocoMKls",
"https://mega.nz/file/UvU1Cb7Y#1qPGjak9WFTVKIdWdwLebrDdfJhuJvS3s_vqI-lb61w",
"https://mega.nz/file/hulGhZwI#-28aSCmrKVwlsJXbuwKDB3J08kCn2cPFpFYoRNnMgI0",
"https://mega.nz/file/pmNCTSLA#vPyFXxAzvpAT91OivkBXmDjAGOo_ekanfcxl4iswkzg",
"https://mega.nz/file/ImE3AJDL#ry_H94TivPdsQv8NTMbunWrm7gtJKb8MLCdieYPcEr0",
"https://mega.nz/file/pqUjiCTI#yllLdtqzJeI1_G2ikxl2Y8kG9AQ8vjtX2SplQ6Mdo_E",
"https://mega.nz/file/onNSBApC#wHJ__yJbarlIWjYtWvM3nclMPjWu33tZFkJeRjaRuq4",
"https://mega.nz/file/kzMU3TLJ#IhOPPQ3ud_0zwkS5z-x6epbHG4QJqqddxwz5FBq3TMQ",
"https://mega.nz/file/5jNDGT7J#DN260Y2rOK8egxSAMvFcH-VqM6H4Rxz6dlbLAXDcPkQ",
"https://mega.nz/file/BuFUBZIK#rl8JO1dqJLsHI0UNcVb3VpVYwlJuh-KpeRXCm4xUqVg",
"https://mega.nz/file/h700FToa#PKDWd2cUDZjXmvAsoh5tczANIbpG_P11fr2faqCSUjQ",
"https://mega.nz/file/ozV2GRAL#QFPys3gLUS8kdBURWZ8GR8WQA-7dMVRJsDwhRPe49yw",
"https://mega.nz/file/J3dTnTwT#pUzu0KYpuSfj9EQlSLymCYf5n2L5w3qVQLZ3Mne5wjI",
"https://mega.nz/file/VuM3DAbI#YRU72xOSBonNZef0nPocbuGelnobILPyDbEDBzVqdZE",
"https://mega.nz/file/5j813QAD#EVe25vK8gNTJW94rNwgjPw6d1u32d3bMVQQhlsWMY6g",
"https://mega.nz/file/pv0RSKpa#lqSv5OGEDQoszSNqxi4nds_5UUdM66ucO5s7cOC1CiA",
"https://mega.nz/file/E3kQxRTY#pN8jtyVEt5Gn7YI9nJ29HECtFwpBDjri_3-PjuT26pI",
"https://mega.nz/file/suExGJSS#5yFDgYS3-inVgDZO0LvSf8aKy3QlotUVhsIui5ZIOXI",
"https://mega.nz/file/Uu9DUJKa#CLTvz3Me1fuBjfLwDiBhRDmZOtnwecrCPBEEpagZG7c",
"https://mega.nz/file/93FhGIxZ#Mul63UQBCQ_I-wqH2wDoxzt6Uzq69Cmw9V99Xi9vGvc",
"https://mega.nz/file/MnE1yB4Z#K0CZ746osdDRiudCjT1kDyGpNh0TfV2c3AvY2jf5wHQ",
"https://mega.nz/file/8yE22CyT#9BzsvXvPIZ20x-5GqxIk-BBi_QtlJmy08cVIKaUaAqQ",
"https://mega.nz/file/Q38gDRbS#uJ3YTzsIg_USRdWyHgCpZ8bWEUSH0aGM5g7XQ_Cr4UQ",
"https://mega.nz/file/tqMzSIIK#tjj8lxHMKwpZ9gCAswhBvX6yjjtQgA82JCoPA-lIE3c",
"https://mega.nz/file/EiEEzQSL#30vaQymCC53WySlTny9GfBWPFCfLuKmF_42JgYYte4U",
"https://mega.nz/file/szskjDpB#vQkk46BGdwuGG6I6dzTN2ZSPcROcpqbaty7oL_1Xcag",
"https://mega.nz/file/hnlzDY5Y#PUe5Q46amscxixG7hhgup0teLeerIdmnf-_xIJ1i0jA",
"https://mega.nz/file/culUDCLB#x3UFtW6Oi1YwLbrKeJqL-9OcMfjiTcEf_KM6ycVpeAc",
"https://mega.nz/file/UjVCVRrJ#S9lKVTTGvlbiUpWaVcmCyq6meBUWi8sQtI6qYqCvMXc",
"https://mega.nz/file/U3cyCY7Z#6gvySul0qc6bN_p_kG2mweXYsuSFXPULVIAluFjCxUA",
"https://mega.nz/file/knM0QBJK#zvp4R5cjWcW7eXTnHuvMs8U_DYlFTkl1UpSWwb-zpp8",
"https://mega.nz/file/AzFhDRaT#6yw9sJqCL96UarYnCWbp-weJQWuCJCBQd2dN1M5X3Aw",
"https://mega.nz/file/5vkVUSwC#Ko0Sa6Pci60bNQQIvlhcu91wqIRnw6msqxeO54okHbw",
"https://mega.nz/file/5q12BBwa#HOLyKg7FhPXVnYZLycNbFFnIg461G3VKqzEH6AATF9E",
"https://mega.nz/file/0qNWUJIQ#2DZURuv4x5zxdBElmjrQ8xPEU6MfAQEbtHDi-jsd2s0",
"https://mega.nz/file/1jEQyIiS#j6gdbO5EJ2KhhC7wmxTBHGng-bOmMemwI4VEx2lQMBQ",
"https://mega.nz/file/0q9jlSTS#kBeSpEfoTEzDmB_E3ZqkmDpPVJAwpLqzaUZDOr64YcY",
"https://mega.nz/file/IrVCkCYJ#TJJ4F4nqsFU6YAPsS2GQIxcqpSLdoND1g5BYu_MqWQU",
"https://mega.nz/file/tu92hApS#NXhgrWkTjwpU_x28mrgbIJvY31y5FZ41__0EFfgHC7g",
"https://mega.nz/file/dikzna7Q#From1SThXFcAc6e9lshfu2rftXTszM-ijwOke-pT5pc",
"https://mega.nz/file/UjsxDLiR#DT6mH1WxvV9yGxl843QlA6SvGt2rY7Q9RwMXPuRwWrk",
"https://mega.nz/file/tuc3TBoJ#Acok9EpfLTI9NolCVRvW0-iGobQ-64827yx9KTlvJLs",
"https://mega.nz/file/hn9FEbLK#P5KNRhT6Ty_z9oiI3RDKtMMXCKeAGmegXlo0P9brSM4",
"https://mega.nz/file/UyUSQYTZ#2KDqA3fc9owkdp55EJvjRQarBSlb5ZPNbkHvEOByeus",
"https://mega.nz/file/97dwkCgL#GgoJhWGYO8Ja1iaNw1xbMMK3ksiZNQ94xWy0fyG16KY",
"https://mega.nz/file/t6UHiLDY#fUEd2FU9Y2qycj4hoseMY0uoHEZXgjaBx2Z9TppcUiM",
"https://mega.nz/file/Jr0ziSKL#iLXS0W0RojwleYUjCTffJJk2T8Ns6QNVY6ndezerm9g",
"https://mega.nz/file/cy8WjLZb#4mewITltocuILFg6dLKyKu__UeXuYoL57ZdIJV_CMLo",
"https://mega.nz/file/si9ViaBI#9aLFgqw8_7B3h6xRf2x9XLOL0Q3g3BB_qg5LhLApi-s",
"https://mega.nz/file/dm9QEDjI#PTkpVFSlOFodZpcYoqmU9Y2ZyXeltQuScjjF-WBHaA0",
"https://mega.nz/file/E6VWEIAD#HCi0jqNEmz_I6BDBlHQ2uTeQgYfvQLzu3eEoLQHZ_Z4",
"https://mega.nz/file/o2t1DJoD#l8sZ-qnwdlWdz-gmVcc6DbSAIhMh6Y8aAY_Y6V1xkks",
"https://mega.nz/file/t3FlkSYT#ynDN1PivioF8njha6N1C3Rx0lKSgrYzjXlSS3ugUwKc",
"https://mega.nz/file/s2MTABLK#2EWyIOPHZJtXcTmEcptWkSIyV34-mro8W-YQ-7t23vM",
"https://mega.nz/file/NvVkmQgQ#TmeHPx0LX7LLi6CeycBwcMpe59KBX8Q4UvF032NpTR8",
"https://mega.nz/file/pjEAna6J#BD9q3zRwF6neYIqp9eGJJKc_jnCkJTNYGRcyKmHEJXY",
"https://mega.nz/file/RvsmmaTT#aZTblSJVltCk7iqGCXES4GV_9iSCIOIv09x-s-GKs-U",
"https://mega.nz/file/JntgGZxI#d2YGfzpDXTItGY5HTSILlW77jKPH69woCX3fyg22uKc",
"https://mega.nz/file/ZrsFSIIT#FgC1_Hdg2XRZPyPuCItHBzcy-JgDDkO0KfvrWTxlwGs",
"https://mega.nz/file/Ij82HQ4L#HKFQX1z_fmrX1QbXei_lv87enEr5sM1aYZGFPS8Bu0c",
"https://mega.nz/file/t6Vy3LoK#xL0A9e8PaTsyoR249GVFgqWSf3RocJ30si7S7Se1V1o",
"https://mega.nz/file/cuVUEASQ#XO9wMUYgmUJTnWhHyMRotg4yP5BGQ-d-TRqYkwC2_qc",
"https://mega.nz/file/E2lnVDTA#dxDBpuPJF7dW3I-DNcW39pLQ49O2yQ5MQXPvO_3qbyU",
"https://mega.nz/file/M3NVWA6Z#RYa4I7BSVtgrRKCjmqxe8Fs4Oz8b4sRbikiiRw6ckXs",
"https://mega.nz/file/JiUT2BRQ#2dSzw-P8F6BcY-qeBVGIBeKVuuqfwmf00Gz8PAuciLU",
"https://mega.nz/file/17UERaDR#ZazDC5vG27ya_ykEGvtkoSLe0aBOPa1el_vKVz5d0Fs",
"https://mega.nz/file/VzETTZJJ#GC7IQzJCKglqex-tct6f6LKnDuFRRPV8e1Hhc5YDDtE",
"https://mega.nz/file/dzVwEDQa#cuVOP2Vq-k32CgaWlh8MnBuwwXksiGpedQbTJ3RW-bc",
"https://mega.nz/file/0zMSTA7B#S0286Tlvmhla5D-a2jSuj5rJT7hcw5-IZXIfawVJ5fc",
"https://mega.nz/file/RiVAiYTJ#VjTGDwB9JgstWEYcUE03SpHsW7tdnjJhNWefTr2kjD4",
"https://mega.nz/file/sn8CDYDY#rCywG5YN3p-_Qr5wAEJeR4zQfGhJjhvc3g0RdBcWwh8",
"https://mega.nz/file/470SHIzJ#p00WwSJM1RpahsVI7ZLeVfRSibrto1MjAlT7Qb2KcUc",
"https://mega.nz/file/IrEW2DaQ#rJGCw5R_m0tRo6zaPDHgxmA6QwOA2hkSSCmVJB5FyPY",
"https://mega.nz/file/dzFWxSQB#BhLgMOvnU2-TU9HeveY3emQAM5YsmOUOcmFclx7HkaI",
"https://mega.nz/file/BjERyJ7A#AjoyiZIUmCCz9J_x43wLgCQaHnoJdMLZvtUmPM5XAy8",
"https://mega.nz/file/Q3dnQAwb#toZRoHzL0EJI8mv_sgX4GD4vdNM2lOmtVTRxoWiBkEM",
"https://mega.nz/file/xnNVzQCa#kx_HzoEjlswsDpm-1zu1U80EBOf8hOsnbiCDUP6oSrg",
"https://mega.nz/file/h3dHFTjZ#z-EnkPZz0hG6UsR873UfW_ZLNtOEtxQ6N6rijgVLO4Y",
"https://mega.nz/file/smtEhArQ#sQ5-I3UZheEtA8lkvez0XZgHHka3eliOCgoeslrfqY8",
"https://mega.nz/file/F31FSJzZ#yOrw_EMlwCLrmia9fqJGVim-aRGwVQ2NdvyiLjRvRTg",
"https://mega.nz/file/pjUUjbYI#2yHAxBiPxfByWKDtCcsmIJBIblBVIkbUROTXVl7_tp8",
"https://mega.nz/file/hnVCnJrQ#66OSCffYMfczmjq4cS_vkPALnK9d-QjNzlu4nwtREH0",
"https://mega.nz/file/8v0hBJYA#BDHnTrdPRpuOIa2iiKK4sCa_lkXfbyte0VWq05QLv9U",
"https://mega.nz/file/E2FQjZwS#ekWIqn7i_codDNlJBdzutWlkT2AkT8jRt31lJj-_RRw",
"https://mega.nz/file/QiUDzJ7D#xnPNqESSNIyNcbYHqXdQhCLOy36KT0CaD85XzvuMDJE",
"https://mega.nz/file/k2UBBbDS#RHoKadPEHK4VKqRm6RLpfrPDzItJZB-_cVg9zGfwfHo",
"https://mega.nz/file/o6twkLoT#qLAUuyS3w4kCXshevjw-MDNzyvG6AxGWp5TqOQboIDo",
"https://mega.nz/file/wrETzCjA#ND47Ps1Z5rl1lItz-B65HVyohCvX1z2TyA2Ff4ArWB0",
"https://mega.nz/file/x3tn3TAa#wAuC4HXjj8BCRrlP6wc-wKzSc3z__sYmNqRKRnfLDjg",
"https://mega.nz/file/wzNxBbAI#_WNQg2sUFpPtUE3P_lg2ikaslE3_KaaQDbeYH8XpWA4",
"https://mega.nz/file/Mz8hmZwY#Ba9I_ZhcDYcVxdKHJdX9u4_1hWCZx8i985JiQXKp-Os",
"https://mega.nz/file/hi0Elb5C#plNZzjDerRs7jG4-J8K_FKPGPlub40L9MHU0bKe3yY8",
"https://mega.nz/file/ZrVWCQLA#jZPrsfDyXGYk-Px8CT_z_rZmcaTwyztmw5PK9giwbTI",
"https://mega.nz/file/YG0FGLCC#iWdmheU1Moli9vt2k6jl-J2HF3bWK1YBDHQQ8G1YSLw",
"https://mega.nz/file/NC012BYK#uzUWtZ7PCGkSnxtof2pNifrGBafu10z2u5ETla9I5k4",
"https://mega.nz/file/kSsziJKT#LTxQBx5ZlXVl1V7HEssquPLNIe4fD-MOgav25OcRz38",
"https://mega.nz/file/8adUgTAQ#T0NkJDG28dT-Eoi0mnp3eVQNh5ov7HUMPeX_7Dh2lUM",
"https://mega.nz/file/EWtyUAIa#nc-YI0_HQ3BjLJodWqNIDkJAOsBxSsj4XBMU4k4mrok",
"https://mega.nz/file/1f82SQ6T#85pT3rA5uCHUWEokYFPTz6ZwPicQJuKlPRMVtz_IIVE",
"https://mega.nz/file/QOsQUI7D#6NYWhsv4kvSTxwABofqgNnmKGtJDA2SQu2GRogRd3ds",
"https://mega.nz/file/1W1FSBgA#0Yv-TvqoPfw23tsu8gGt56K7Yn43GWAUzGTZIk32qTg",
"https://mega.nz/file/0GEyRbZI#QVBuef3JHEGxIbOa-MGUr7xKxrOpYOAeHrfw4gKQ1WI",
"https://mega.nz/file/ReESHBRL#r6czgRGYzfOF98AjBFrTIukIsfofwoMjmdI22qx3Xog",
"https://mega.nz/file/sbM1UayA#pyyzdXIzFzx1a0NjPK1B7ionpsq0tbf34w4IUMNYdsQ",
"https://mega.nz/file/gK9lybKZ#wuu0YsMh4QkcTsR8wO1BvMm__y7l-W6C4CMtBVogjnc",
"https://mega.nz/file/JfMWVILA#TEQYT0qxtzr0fkdLeIY2s0lV4cj7kECb9aOLl2ZWt2U",
"https://mega.nz/file/MfVhiSKZ#_VF_9KQ4eUx218HpuqP3NYQGSoAyUKIwjyd-D1R_Dkw",
"https://mega.nz/file/4TlVVbSA#9hR8BFT19EST1QmSd6_LtI8R352RVSkBoO0J3sq_SyA",
"https://mega.nz/file/xPdUCRYZ#2b6GGu8lxbKN9YzmzWQQTCONDYVaIbF-jf9WuLk2yxM",
"https://mega.nz/file/5O1hiLqS#L64SHPIwb-Hxyf1C7yXVj0gLg5V1WAIs7Iry47Lx2qA",
"https://mega.nz/file/UDdDBZAR#frNug92WvZPQjy176K7z8mS4xgaNEsFr6llEQpEwa8s",
"https://mega.nz/file/FC8hGZxS#W8c8NsxdjZD2T1OY1Ft4nQT32cZhHzX7ZOWbnTkshS8",
"https://mega.nz/file/pbFCEZKB#QjmpXCIrS9QCnGrlIFEeZPjKupdjNlp-OfYaoYJipRM",
"https://mega.nz/file/gaFjWCgY#d-d5_YVe43huK82O9XprKz339v658V0NjTxyRejVdUI",
"https://mega.nz/file/cXUnBJSK#ZdYYJXKJcCz7xyEwsDju49tTvNDG9E8SBrGYQ1ZjONs",
"https://mega.nz/file/BC0BTABI#sBi4u_cHj8LQIwxkxeM-ivNWovSm1b9wPUYI96QH6J8",
"https://mega.nz/file/1OkiASjK#QE5NMmptmoPkDE4tsbQKo3QkCtXZ_rdMRNxNKGk5P5A",
"https://mega.nz/file/ZSkiSLAB#paMsPNOxocvGIIIcvkzvp_xbpEqF_yXwobQA6ga3QBE",
"https://mega.nz/file/1GVkFQaJ#euppQh1ctmlCVyDuMYuCmJDHniYlUtlntQj03405bjQ",
"https://mega.nz/file/UelkzQyZ#Wzhfp2c7A99XP9Ezc_Vd44WSTob0hF3qDMBLYjC2ClQ",
"https://mega.nz/file/IHFlWTLS#W13mphqnRrJ-RJVsyZqgX9Q72qPpHN3gllPcOmE7m70",
"https://mega.nz/file/AeUU3BAb#XQpgOYXNDwm_OmlZEgTvhpXrK6aMyN_iazJjiPA_faI",
"https://mega.nz/file/wKM2xKiJ#jVzr3ZEMIqQup-Cx3DkB5VNj0yLI9AY4cOaAzLVc8XU",
"https://mega.nz/file/0ftlEaoT#cNheRb0Gh3gG9ogrTdOsM9nAjWYAwldDYykllSjz2mM",
"https://mega.nz/file/dSExHQYQ#ff6xz6DTra57CZRYTCnhzru2_vhP86wgD-AnmpDMplQ",
"https://mega.nz/file/FHMGVYzS#-UnU9J7nBRCfOCsvqnJkcHVpRtyebgrJ-fycHSswWm0",
"https://mega.nz/file/YXtB3TTY#j6-lnMpQfGl3m0Lhe4DmNG41LK9X5I11rYZecMz7DRk",
"https://mega.nz/file/JKESDDqY#dMSbtFMFzs41TjPJuFO9pVGeGl7hOMOC-ppfLBiNIRw",
"https://mega.nz/file/cSUiQSBY#TFTkqVI0A4wzNW4EOlDIcrSJW9GGEOeZchy4Ze0SZhg",
"https://mega.nz/file/oH1gXZ6K#GajpL5Twxn_gcggtAmIGdJHP0zJGI9D1zKdsBZUyEnE",
"https://mega.nz/file/8S0VHCiB#Xm-MngCzMoa8cWcObQa_r9-b-8T8aNxjIZwFWILmQBg",
"https://mega.nz/file/dbsy3LhA#7TeP5XfUpkY50yz_CpogFKxKPCthQ9GugILRBndTJHE",
"https://mega.nz/file/ZWtxHYCK#3dmxvAj0Pe9z3hphFRLqBp4c0UUQ7gZUTKmMto0Jvis",
"https://mega.nz/file/ceNX0AQZ#-bva3MuK7N5YrPNvkQ25Ag007qhIjaxDPVqDuU2xJpE",
"https://mega.nz/file/NfkHSRKa#wtK58MSqAfKLFHlZ9ac49rsmTXRcjX1kYh3Ut3TL_qI",
"https://mega.nz/file/YGkUxbjK#ssH1tlYxSue0ZeNsqq7x89OxWAElUQ7L9NBH8nVlyXM",
"https://mega.nz/file/FLUGQYpQ#W9Ch0Uzz8c0za-2Y7pJwqqB-SPwiLk4qXhGPFiizkgg",
"https://mega.nz/file/YfNChJCA#bitAUkBtOLxeuY0lUtasLx_7tdGfMI16CC9rkigkmMU",
"https://mega.nz/file/EWNghToI#32DT12pqLNF4nsmdktmtrLFk8V5YZ3rnxvaaaIpOYa0",
"https://mega.nz/file/JL1EDSoY#qzycqYE3YVgicyAkNdZDynJh1h8Hj0KOKAdwsttDN0U",
"https://mega.nz/file/cOcTRbzL#PtHnyki1nPZeH511Yl3Djy1b4JLI_MkZ6ya18ItZIUs",
"https://mega.nz/file/RPFTXKYZ#zFV63XGqbnn2L5M58gK0A8J_lWWY--dOTNc-hl8XJqY",
"https://mega.nz/file/QflBWC5R#48wVIUm9mXg4urFkQeeITTSQpOoR-LwBOC92DNkD8OU",
"https://mega.nz/file/QfcDxBCK#Wo2jahsjdvdP2q5EQZMlYTTkNLMeFuaEtLae4wPqEfQ",
"https://mega.nz/file/MPFXHJQT#4iU5uLOaZ8RiKfZT2jlMopR9edht3zkJLRB_DSbWIRA",
"https://mega.nz/file/lX1i1BaR#_72tdsT9D6StL3SEX-uvr39qk3oVNExgqMBYVB-vT0Q",
"https://mega.nz/file/1fNiWBJC#HBzlgukv1mxaFfzjXsKLjyDnMRjithJyfr6cdFl8uEY",
"https://mega.nz/file/JOUUSC6Y#d_PQLK5vf4LPyaRbp2Ohv5etSn3EwAYTMJWUUqXoBBE",
"https://mega.nz/file/sS01lRrY#ulKzirXCNUyP8nwUVQDSdMJEc7OsTGSWEX4Jx_s_rUg",
"https://mega.nz/file/BXEkUJLD#J5zNVPLjeq0dVqkkIOoGiSra_twRlD5SREgk35WcQn8",
"https://mega.nz/file/pecznZRY#ETqMds0FdWpjR4dfkDdrZpKEIphv4cmb1sLm18x2OFo",
"https://mega.nz/file/2MQVSA4b#Rn3eOctCAVHellMOBLZbo3ZzLDubhJLs0gKCp4VZ_xc",
"https://mega.nz/file/zYpTWaqS#6BfucZgPFWul8Jvw_WoSWSz2KRaFC_OYg_dd79m26Ao",
"https://mega.nz/file/mcoUWD7S#NPs06G3S79s0Tn4-eX1EG9AmuIiwKFyVV3xDBtZQCEM",
"https://mega.nz/file/mEZRxIpa#FOYTmUtEeMJSy-0XpN0EHwelQxjSl5tjrdBZzXWkMqM",
"https://mega.nz/file/2AQUEJpb#JS94IV0eyuzqCPCTtmcURiEhqLn8lMDQ1FPTQ-k-LUA",
"https://mega.nz/file/eA4gBbBI#jf9nt093FX1KIwN29EFN49ID_cZwdrbxuwmoM6JgJtI",
"https://mega.nz/file/XE4xmCbD#O6zxy1ao3FepVnaTCJ80_3X7bbN73AdIvZWcuHJ-p7c",
"https://mega.nz/file/nVBBwBRT#4qqDJ07TNYll0GSwcYilgBfB0LFrVdAFQ_H2ohmyD1Q",
"https://mega.nz/file/aQ5VTIJD#2OGywLX3y51ughPu2zqJLwbsgCWMFtrGQqjhqKi0Ygw",
"https://mega.nz/file/vRAnyJBR#iLopLNUKCoJ2Z9x8YzrHZYBVakftEjj6OBaf2tpSN4o",
"https://mega.nz/file/zRZV3aDD#etMpOcRJOyS6XAU7X0bGtiDQ_xLJSFVJF2gxBDcE0Q0",
"https://mega.nz/file/XExyVCxB#sJ8i2-RG80E5LjJheCFAdHbah0xATWaXPF9tjSN2s5s",
"https://mega.nz/file/mMxACYpT#YKSb2vC2S5HjXyRJRJR6nDuNsueRn3BCLO---Giqm8A",
"https://mega.nz/file/qdpwyCRC#dyEOLFFKNhqYgA4E3f60mGn-dmB0lKx5_fzJ7UxDuo8",
"https://mega.nz/file/nIxG1KhK#S7xzHC8jsh9TZ9FnXfdmYGvOTxB_tBOz_HpdopxiYdc",
"https://mega.nz/file/yJw2FShI#TpnADAKhaNdSPnYfOQL-n9QzynPP5XXzXZWDEsDn2Og",
"https://mega.nz/file/6QhwUaaa#LSz_PwSzIfgw88PRsB-vBAaGM17NPCBz7QWBWhzYKAk",
"https://mega.nz/file/aNBHDKiI#SBkRCDvlF0wRfqKbgiOv8WJ_fC8oXBZN14pl6x4w058",
"https://mega.nz/file/yVAzkI5A#cvQcHM8aZ4SgvgCiZARnOdskhMVr10p7oclq_xlzO-s",
"https://mega.nz/file/nY4GBT7Q#BElqNOAkdjiih51hIaG2UV4d2AdUKlYTVfiabh60aE4",
"https://mega.nz/file/uVgBUYbZ#5nt106JS4y9XaMyrce7phhxQcEq1Oj4GQ8T7HQmwX4g",
"https://mega.nz/file/uR4zXKLB#5qAtl_TlkAPBNGQepfs8biKbAnYLFOx8ghJlUmzG_f0",
"https://mega.nz/file/WQgnTJZR#d4m-8s5P0O65u96DbZkSYeKpAfu36bfl05ILWURMN6M",
"https://mega.nz/file/qRhiWRDb#LqTPXe9ebIe6EdoeFfqppxP8W-BX--iSWNQFz1Tj_1I",
"https://mega.nz/file/6EBXFQTZ#VKM5gRmT6miYQGX-e4-zvH5fg6jWUUCP6boCOmf2Y_4",
"https://mega.nz/file/ONZ1BDpS#whJn8doL50L2-PoZMhFCemlHTSRSeYLDFHKyWN9g3qU",
"https://mega.nz/file/zEQECZRb#avwLSWlKyNGGoDEQPqTDSrfTsWtie0a29g1ZuWH0C58",
"https://mega.nz/file/DVByybqJ#QzgkuCe7RuEUxrdTNuQC_5vTc31VJRP9lY1nmU1XVfQ",
"https://mega.nz/file/WJoxQYiS#ve4PLgvHTHg9TiD8sR6j_2zfjzD2GZo8qAOksU_ADUQ",
"https://mega.nz/file/OcBU1LSL#-iIQ50oAWLYDt1oifkMOer6XRJv-MWNDHvFZ67E6lN4",
"https://mega.nz/file/aU4ESYbY#gSZbin7tE16RRYmOowQSdRrb8QZKag5hKVzcanhr3u4",
"https://mega.nz/file/HNIxlZBC#r4VyT5pE1Vn1ZHBAROBltn9CSX8kotI3SEExTggbFUE",
"https://mega.nz/file/GEYHRbQQ#RohVB4lvEP2aUg8QgagOaAyrTVHaPp6eZvLK1Qk9fo4",
"https://mega.nz/file/aJoBxJrD#3MCphKup_e4PhtvR8BNiLX_j4FS2mCGagptQF9wQAMg",
"https://mega.nz/file/KQxiVLyS#cPnil1vi1JG0T9LJCVxMsDXAkrbYoKnJPDOeMoY1958",
"https://mega.nz/file/2EJjWQwB#O63JeJLoKZteB2vIxRfmMPqXKUZCnpOscPkcrPOEi0Y",
"https://mega.nz/file/HM4EDApR#5zYL8i2uJgJTqse08D3z0Rtb9rhycO750Pqeu77WXk4",
"https://mega.nz/file/SFpxSZiQ#oqO-oVkbLNCyPrDmsAyhlZQcEWG-ZpRas6R1Ob6t2jg",
"https://mega.nz/file/DRxG2Y4J#7i4xbxH2wa9R2IOOP_Bxxno9b5eRXZKKdsH8D8PW75A",
"https://mega.nz/file/KQBEmaoT#v2CxnGPnOgVjzCNHnRdTKZlqmkBr9ZHghF6DaoZluy0",
"https://mega.nz/file/KM40jAIQ#RCfc9Nz81mBt4sq32ETa4KYv_xn66J_F0k9mBWrgwWQ",
"https://mega.nz/file/TFZgEJ7Q#XplJTj4ZOh2sIHI2CLwMmN-wFo4sAoJ3sfacK22T1xk",
"https://mega.nz/file/bdZBnIwL#7ioBmt3M5AEDTMIeknHYqJusTV_2nGqZ_7_uytzWdnE",
"https://mega.nz/file/uEJV3RxS#vAz23ozjnLR_yKnuL5q_uGLLTQKkVVApeIkW7vTa4OY",
"https://mega.nz/file/qQZzkZLb#zhr1CL3Ie2dIsEt0tXk5zYF-ypZ9-gExT0p6nbPnpVY",
"https://mega.nz/file/CUBG0IAK#XvjHgRdK-dpUmDvv9bIlQGO6furXqvBORL82otJ4KM4",
"https://mega.nz/file/LJIEkCbR#uQRMYeC4Pqzf7COg8zzAmwNT_6q8DeG3NdfA3UYVwBQ",
"https://mega.nz/file/XVRzSQ5C#ldoonUcn8nETwvsIjTTxCczyzWulxzhKmxQx51tY3gA",
"https://mega.nz/file/nJI2xJ4L#SvoFNlV6R4Vg7zUskk-YltmTGFbSPMWdvyx23URtFe4",
"https://mega.nz/file/CZhUib4C#t6oYCWfAILZvhZz02-IPw1L68BzsxkdTUHXO8TDSjxk",
"https://mega.nz/file/6FQiSLxC#35zShVMldW1JbVGtho3A5tdWaOatDSTb9pstvSyt_xw",
"https://mega.nz/file/fZJzFTYa#gPg9j1am73HKAd_AB9_5uXwb02AkzjDEsR9lq6uSu2U",
"https://mega.nz/file/R2hSDDjZ#se5yUCiTJ201YwWgqfNqOe3bba5P6ppDEzsHbbiqsNM",
"https://mega.nz/file/JiAiGQKS#shDL0SSTX8vGE6QH0HWITBwIReaCO-bEZ4eexn5SS3U",
"https://mega.nz/file/Nqx3lKIL#8NCkhgADqteHF8K8WPua0ZHBsmEMwz3YFWd2T_ZtV_Q",
"https://mega.nz/file/VnIACbrJ#HzfVf5gFzTbZmZ6o6fIIdpAwyZXz11_esqHChdpy5CM",
"https://mega.nz/file/JvJnzSZC#oPoe52DI1tFnfb4g4wCy_8D6hu_57CgHgvjIyYJ4nOU",
"https://mega.nz/file/lyIEWIIJ#nBGnpPtcmsNRkerxdFK2eHFEc5yguZfmbh_nlOiIuEw",
"https://mega.nz/file/Aqo11Aab#Ut0xBIpKw2-esQKeIYbRrEh3UubWSW9--TyugnVYtkA",
"https://mega.nz/file/wrxxUIqD#hEAa6OS9Q4UgdK8t1elujFBuNRW6wUjquZqJGVQkV6A",
"https://mega.nz/file/hj5SGRxD#Snexq0K9R2pLhIS0wHdg91tA2rf9iKkg5SzDJSmOZEI",
"https://mega.nz/file/xrAX0RDT#MXsD4gYw9i47y0EYHMicsemkQDU2dbt8A8pDDQkt3tU",
"https://mega.nz/file/I7pD3YyY#EgKXxvQmtYjU1lIXqoZSaA-jVSyZ5P-bfSb5JGcfwkk",
"https://mega.nz/file/9nJklLTZ#CderFuDDBJfO65C__q6pyfcy8nhZm2N85nb2ur0tXyg",
"https://mega.nz/file/IzI2ELLC#yPZtjO9SciAmCoK9ld3QFN6b9aZrdUMEwKFeaZOdjB0",
"https://mega.nz/file/wihVzQqI#tRxbTZpr56TB9xYrz6sHktAXHduXeLdrNci_d6SQiMs",
"https://mega.nz/file/kzgjAL5T#y9vZdmsAGL6s41un0LEmbxniDVJwdit2qyD76Woah_o",
"https://mega.nz/file/1qgCHLKC#QdvPk1gVM70b_GsaBwqwn43Q_gtx6fxb175tQKSg5Ow",
"https://mega.nz/file/p7ZyRY7C#B1p9Ibm9b1_lzN5RSUv58hh2sLZMlRxRDsP148hxTwU",
"https://mega.nz/file/gi5C1AQS#cHMaQ9SHu_vdDPVG325gxhHOpDHDJpgp0S7nTpSkfnY",
"https://mega.nz/file/diozgYTQ#leYz5dFM5Sj1xZunYoQ-agdvnOtq3PRrG1z_Gxu2Q4A",
"https://mega.nz/file/1ug0QaTK#10_26QmcTJdXAhAPFh6gApgFHFgewgLbP7vqt4QPwns",
"https://mega.nz/file/ErRiEYZR#YwtSPVaWXcO0J_N20WFjOSZo0RtOObbdCVz0DYhasyU",
"https://mega.nz/file/F65BHYrS#hsSWFFIxmkuPbKLnJZ-SH_j5ySuQQ9Wz2ZvRaJaHpCs",
"https://mega.nz/file/A24mkQzK#CngcRUeBkzGKMcdvA-A5gL89czQmx_bxPd_OBK3snxA",
"https://mega.nz/file/5vgHGQaI#JFccRBMj293BHzByydBoJCQMH-cqHGx4FuT4VQVExbc",
"https://mega.nz/file/92xgjBBI#SQLMz6sMc2KsItPosu5jJYbjaSeMTKTK4P_Q6HcAphc",
"https://mega.nz/file/IiADQIKL#4bdDyhqz9UXRfozs_ZZv9zloM1AX8qeNptidVoCqfn0",
"https://mega.nz/file/Fu4XQYTa#qZoNR8rqrps24rs3IhHJdclamEptO-nDcWaS2UVhUiU",
"https://mega.nz/file/0vpGxKKB#P8G6JHWIoiEt4phq7apGho8138iR9F4Pw9hqvSMfVEI",
"https://mega.nz/file/RzB1RazJ#UY9rfN3qmO3tVbel-rmy9YCcKdGEbC8jyJE_KlidcD8",
"https://mega.nz/file/NiwQHCiD#PPHkp6Q78tem4m_5EBimP1VCz9p4CQptDrJ_rDIpd98",
"https://mega.nz/file/Z34C2S5B#8YjpVNHHx5mKpDlwz72ZvwHkNiC0Gk6JGKJk9W8ruQQ",
"https://mega.nz/file/cz42CIzI#v0E5nRodHxQBK40JEcGoQ8n4erkPurMWi2orMrMuEM0",
"https://mega.nz/file/9mY0RayY#qXIU-dUCW3QxPLwXZlXIVc1qbj33QCQGjsHQ6NL4UDE",
"https://mega.nz/file/s7hxHbwK#0CNEBL-Bnp9l6LwyoocpUo78RuyqWoT0vwRO5tXMfK8",
"https://mega.nz/file/UrBXXIDT#VxFZht8D0uyMxdQE70jPYTTYOh50Br8xh35fg3S0RYQ",
"https://mega.nz/file/86x2GQ7b#bAROD279xLO6ZVE_JMqjimqO21ZXDxdQ0nrwsc903k8",
"https://mega.nz/file/lmxFDLQB#3xj6GhPLU14aFwedipyqsunCOd_goeCvsx0ZTRqAstQ",
"https://mega.nz/file/NvgRzATY#sPq-j2IKPczxIU_C5EL3rMHyAncQgFna90TlCqYhUoc",
"https://mega.nz/file/giZ0Qa6R#bWMhhba3TWu3zrSC_Sw4qpwJ_gVNMkN2zgxyl1mbIH0",
"https://mega.nz/file/tmh2UB6b#yaRRaq-1duKLm1UtnzE77jPDTPpmhcMqt0G1i0upyhI",
"https://mega.nz/file/8nQxyDLR#zqY9IVTqD6PgCVwiQmeIi2NTRFIbTDl31sSEKeffaaw",
"https://mega.nz/file/YqR2VbDa#n4TMxKOnoQE7C2BC5iYuZaAga8jgwu41mDyJ8T-uzYk",
"https://mega.nz/file/gnpz2QQD#t2dYfD7KyDIcBHbh6o1E4lhCbkrnyT2Ezb25XxOBM9w",
"https://mega.nz/file/h7ZimJBR#Io_WNZqN7tqqhNLLyqPQt-TEanLnMqojV49JeJUxvYQ",
"https://mega.nz/file/wjglULxS#Q8iz2qs1YCrjdYSl7-Z5Au582YK16PKxcbIIHR4JiQE",
"https://mega.nz/file/M3gkwTgK#XNwTKpt6mLFWg7MQ_y-E00-1zH5JvUW8VVDWUpzggPw",
"https://mega.nz/file/NvgxSR6a#S93gHrzzRqRIiupdNsbtYuAaZE_B4qviOT4ajKwJE4k",
"https://mega.nz/file/VjhkVCIA#jia-KVpjz9kXwmr4F-XZMkQqIZPLmPWn618MgfFyiJI",
"https://mega.nz/file/F3ZBXSLA#s7IAzXO1UwnvfxiDBK7wPcea-1nkEckFDfF0cTvUSfE",
"https://mega.nz/file/JrJinSYZ#8tFJdzzZwxOAv5Pr0yjD_JIJzU_MiBmRDJ7YEKbe3b4",
"https://mega.nz/file/dvJQDLDI#pAxt00fdOXK-Y0LnbKM1K8YchZ587zNwndBMlbmH9Mk",
"https://mega.nz/file/p7wzDISY#w_-iiS-Ly0_LHGJuPdLZOG59BgRmgVmqJnDWw7OKeBU",
"https://mega.nz/file/t7R2UYpZ#JdCOu0AI42R7F49hoSDQ3BlnRREHJpslVtPlF-goF2U",
"https://mega.nz/file/p6xCQBTI#IFtF2tRgYbwPWj9Sl96UlmSFdTGsHSObq2S8o3MBirA",
"https://mega.nz/file/Iywj0QTT#tVynF_euE1clvtAoP_CAjwlh_y8YnDpOwwb-hVyaBIw",
"https://mega.nz/file/RmoEjIoI#mSOCi-Vv5ID5fq9Q2pOHBSMK1DL9XxbSY-L7xP4qq28",
"https://mega.nz/file/0v4zWSQA#vc-H1myQdBwE-Z9Plt3ofUZcCCgk0ay2YV47o6fxCfA",
"https://mega.nz/file/YnwmWLgL#WNQ7o_et8Lo6wHMrVPw4dkOoQ_hD_IheW8lhmm1yK9o",
"https://mega.nz/file/xixATRpA#jpMsMci7gKlNj3hIeDNfxPrjge_Kg2rgSp0XWNj1bpk",
"https://mega.nz/file/9vhD2SBS#ErbVn3n5C6kwfzf6_n30GZG110H9870Nyx_OHJj5Ekc",
"https://mega.nz/file/1zABSRyL#Afefk_uGQVzW1-qgsQ8A1Pfe4DOIHzHGx68xm0jZtuQ",
"https://mega.nz/file/QyhggTwR#VHIlCJkmyWZdKZLb9QsOvc9fR0PPc0oNO0LbXLgRnGk",
"https://mega.nz/file/4nwzlTgD#WpRWtL0vi3OyhjdBs-HihzLtP-_M9t-9CftJWHU60CY",
"https://mega.nz/file/AmYk3TII#iSzK-8WJs1HNhIl5tHLQak9S99rbgEQ37dtzudfw0CY",
"https://mega.nz/file/4rRD3TbR#63E51Meu4p0WojcmhCDljerP50ikPuKokpHEqQ5q-WI",
"https://mega.nz/file/Um5wzQiQ#eD9ZEvDe2_EszfJDK7all7tv1HLrAgh843-_YNG9R0w",
"https://mega.nz/file/VnBWlLTD#CIG337Q1Ln7tdwxN2YocKsineXDnNnUnoFytlf6EsmI",
"https://mega.nz/file/8m4iXTrR#hpCYOnDVQ7hq0ysGfqS86PBOiOD7z6HpzD8CdXzlRDk",
"https://mega.nz/file/ErYCzKyR#HqnO-ilDPxDfhx4l_tULQ6G13zpo2YtwB3NECwUtn9w",
"https://mega.nz/file/4q5gzLQZ#dCzTKAzBzFItHlrnHO84h0farBA72qNm_UHdj3u8hKw",
"https://mega.nz/file/B6ojRapT#LUtY0ofE_K4HYcCTuQxzxnpqdYZ7kzkFJ4wgxgxOnOE",
"https://mega.nz/file/d3ZCmJgK#MmHvsdnq28XvOGCnWqG_Em3x_eLsVfZfg9dy7SvycMY",
"https://mega.nz/file/wz4g3KCR#o-NnPwF58RPyoaj0Xo-ftv-tXVRqH0KfMSZTuOqA9r8",
"https://mega.nz/file/Q7431LQK#IZfe-nIs9U0gg0uCvJXw6Hw4wnrWRNTQVEf40V7oi98",
"https://mega.nz/file/czZnVAqb#NyaLTlMGQN2Fc1wp48YGFh3iAgsrRnq3BqwkUeMs-EU",
"https://mega.nz/file/YmRiXAyC#W9b_pLcMsPwomC-xRgCwFFTpAqJFg62rcXRdpMhJ7rw",
"https://mega.nz/file/FixEFCJT#BvW6yJExpYp56T7ALPoFZT6y3obiFniOdsizfgNIw14",
"https://mega.nz/file/ougXgaIY#EpYikwxfUGMyEghiYrcl7w6UAhECocwou6qoqSYT9jU",
"https://mega.nz/file/AyZn2LJD#wS2DbFR-HqbteG4Vwy4UO7RALnKKbYz1mh9uUOxJNwQ",
"https://mega.nz/file/EvQTgb6C#SDQ0zfB3dVDKZJke-dFKPwqXby2WQ-H027P33E-Y_yk",
"https://mega.nz/file/Fvp1kDhA#38m-hGarrIZvMHT0hh_Ncylsg960m33k504t69d1LdY",
"https://mega.nz/file/Iy4V3TyA#LpVSP4wtDYSSkhq2xKrdHiwEP0jGW89hMYO2sgCUYk4",
"https://mega.nz/file/tuxQjTDa#zRtazUeAS1f5lPuFO9ZfYFa_BM-wqUnxCZot1CvJIbE",
"https://mega.nz/file/tzhFVJLa#ExbsetRew3p18dQiHawJxVm6LJFD62-qGbRiG-jwIag",
"https://mega.nz/file/EyhkwQDR#mUFhyh2ae5VKf9aBHflMsphjpgj1KU6kqfIUaFWmCtQ",
"https://mega.nz/file/lzBmwBTI#NsJ_hOP_m1dkueFcXJELCAcOhZ1AHNhaiHRxj6TEwl8",
"https://mega.nz/file/8JwhSIbY#26A078GCXXkcbDn0ehakcRWxAkYukrn13Z1s8SEwm3M",
"https://mega.nz/file/RUoQxQgJ#6DpKGfIsTeVa4waKeb3cS-2HkxkR3OJY3cSBA3DM_U0",
"https://mega.nz/file/sV4x1TZJ#awQb-ynl4UGPWoPHKhpgjg43SOjqJolDYGSCJuFTw7g",
"https://mega.nz/file/lBowjDBL#9v7R9U_-5lQsA_Al9mRLWrg5ttKRU_-esC3yprj4GPQ",
"https://mega.nz/file/UJpm1RpR#cgg_Lpr-mbuDZIstHFQRWmoWcPiBRe0RhYocH28hYBk",
"https://mega.nz/file/EAQjHbxa#5fCRmPOui9EaDEfGueRDHIOVY-zPcQ7CHj2vjlcIyPA",
"https://mega.nz/file/NAhzkZSJ#AwdwuXOk1WOp32fPFARAKXoZXbMVDF4kOXUfh4SZtY8",
"https://mega.nz/file/ENI2yI5Z#d9VaTC2GlhRLZ3bIKR2JUBCEgd1cfBku-2nLl1V2bCA",
"https://mega.nz/file/lJI3mQbC#A3Tp3GKds8Ym0phVT6WP2snNAvShupdOGF0fEZkrHyY",
"https://mega.nz/file/dVQG3QCL#gIi1XTvgWwWBOaefFC0JMYAkvjURWki0RO8lqqlHvms",
"https://mega.nz/file/EUoGBTLY#2HD-koWPan1SHT4MLllqfiBVdA5Y9togMEtUZ_DG2s0",
"https://mega.nz/file/UBplDDZQ#SfhYnGneAU8hwOSi5aq6ztysGCJp9qTqx5cinW_E-Es",
"https://mega.nz/file/EEJlQCwQ#dV8tHAXG35-k8_vuaFjzR5pinJC4y2SShoXZzYc_Iug",
"https://mega.nz/file/NRxWmIBT#apBxr_BInQOkSu2FWUFEPIyyCCurD5KcAY16pMRDmFQ",
"https://mega.nz/file/5c5zEKqS#_gLZoqyyRi_JMDWmi7U7GrRQ9oh1OHgoi7BIp7Vq9GM",
"https://mega.nz/file/BNx0wDKS#_SHUJDWwnbnc0CUXlQ6iHNP45N_aytMxvg161htBAuk",
"https://mega.nz/file/0IhmTb6I#eWcfOrNNkdBjAx0W4KlnegsecvkvPgKSNpYgJMDJPdc",
"https://mega.nz/file/BNR3zawL#q9ZNCkASc34v4uUVtOaPWIm56zBp1HQDOcnDvhPg-9o",
"https://mega.nz/file/kcAGHZ7I#e48TiHMWxeHhyweTC9t4AqEVnHecLL6riUQjw9EKF7U",
"https://mega.nz/file/1FJhiChB#ZF27qE6f2yibxPzn0paxTDHV2sg6Cx9iJlUA7F2s0i0",
"https://mega.nz/file/sAIGgDZR#GCM7Ub7cKROybIvvw8giCpKmA9UiDCjLFqjxHfU9IAo",
"https://mega.nz/file/ZJpHxTpD#ZfUCfqbgnQo17LO7BUU9mctr-OufEEPSDHbkTw7Jkj8",
"https://mega.nz/file/BMJCmBQI#7cT1MGPWDGmpYZeXBwRKS7qySssjLNJhzogTPhc8XJw",
"https://mega.nz/file/gUQ3CCKZ#m7S6R3D9y_dbn3bEtY8Yg4lySPlelren_nwa_EOwQ8I",
"https://mega.nz/file/wIpSSKJL#YU2YFS7nSvbyYGYXWD5tLs4UlkXZsatZuDqT-P2g8JE",
"https://mega.nz/file/EBRRXZqL#CY8faV3Yq_c0PVll8L_FlVVWlUBRSVPUpYyNmb9ZBW0",
"https://mega.nz/file/4Bgg2SAa#dRWrA7BvuG_4ES5dyH5yaRqE9pTDhbojhorSrCNWEzA",
"https://mega.nz/file/8JhFgKjK#jDz3X1nM1Cnp_xW1oyy7Bsb0JhchPw_SgbPl_3qXqRs",
"https://mega.nz/file/NI42XYhA#KAgm7EOCTfiISsMXsxe4vwmjon0AHvXiCVbpDCJlceU",
"https://mega.nz/file/0YpzBKDK#gx9Bd5J-gv80HLaZbOMLV7Z44tCtkbXojaYeKeC5EZM",
"https://mega.nz/file/pEIjkbqD#V_HZ4n1YCPxaARHXvh39huxs9OKNvE4Id1N0uZkZB6I",
"https://mega.nz/file/ddZU1LbD#oVgvAgk5LiSIZzjMfC-vCNYcKJcmfJaZt3eqNJxR2Ks",
"https://mega.nz/file/REJFnQTB#h0xskctUt9IFDEtqmPlmNdeDGHi_R0CnosE8mF5eLEk",
"https://mega.nz/file/Edx3jDzD#4FyMNzfjBHeXTRJlti9pI5RBk7_I0O_6oPVL0RLh200",
"https://mega.nz/file/AJxSmYQI#gizmJLDHeoJQE8EZMtN_zMuBNr1FJln3dO48y5Abg-E",
"https://mega.nz/file/NNAFWRqD#6F4uYWs5ZIB037a_E237XeTKrfFoZUlF0YS9exgQnxQ",
"https://mega.nz/file/kY4AFAqB#6PESB-LaPInC7pEgDAhF1TUZSgb6_m2AaeGd-LLY6BE",
"https://mega.nz/file/YQ5GBTIC#fjm99fvkQt_ZtlIUY5WBB1bNNIxGoYJh5NNED1iliL0",
"https://mega.nz/file/4F4nDSjR#y_DwFGOGtxAQjDTwD-JGVdFQeJZ0ZNfZv5fmP6d9vPc",
"https://mega.nz/file/oB4xABrQ#C6zq7ZWsv9sR1aAlWcEnC2IikBWaMwRkjbvr1H-GUYc",
"https://mega.nz/file/sRo0AbpA#_eLOUmSnvjXiceZ0aEFoFkMH41XbOcTPceAoCTwRck0",
"https://mega.nz/file/JZYQiLiZ#_vRl7LPLrHrmjXr8h1TYksPJtyBPOoXScsiWv3kEw3U",
"https://mega.nz/file/dV513JaC#HjlJo7j87e5cLaPQ9UhdhgeETkgQkRcc4Ya-_SSZot4",
"https://mega.nz/file/0UBClJja#v7Yxbz16sREMLeuZGFxe35V6ulBY9Kcc5_XRP7TaMkI",
"https://mega.nz/file/BExSDboQ#sH9cTlH2NivNdtNfmjkxg4N00cMaC1oNjjm7grCmWDk",
"https://mega.nz/file/tcQSmRoC#llb8MhS5J3IuuarOtEIMze2CqXVUAthKeYwGByFRMzw",
"https://mega.nz/file/1JpT0A7R#Y0h-xUPJNIpNyg80KbxBErxwIOcO0rGDx9xlUnrSI74",
"https://mega.nz/file/cVo2HKLC#RFpIcsTQnCTBzyEVBFVtXzE6U2KzajCMUcxukZud7Tk",
"https://mega.nz/file/y8dkXJgb#KgR9KivAJ_i2wvNabaGKIJq29rDR-d0kMiXOI8ED-ys",
"https://mega.nz/file/mklAAaYZ#_ggwxStBmAib1bnjoLWWomTeMQ-rHLNYLuIhwI6zpZA",
"https://mega.nz/file/X00BnI6J#EYMUdk9efImCs-3lGRdQCQMU8T50UlOAD_GygO3nKdU",
"https://mega.nz/file/OkkxFACA#W6MK1isWYzsLlATgYZv4iwsoUf9smUzdzcjIAnwJxiM",
"https://mega.nz/file/3lNCjQKQ#KZ1DwTf347gnzSK1UxzK2ciw542M47Pr3TG_V2mf86Y",
"https://mega.nz/file/38dEGQYI#55YDOCAtuJuqcT0GpP9xXRjhQjrhPP5rPQ-XDz3wneI",
"https://mega.nz/file/O10GwDKK#7AEqtExCyMzI_g4QoNWti0lElJZCq3-t2mRdn2hpB8w",
"https://mega.nz/file/CtMUgaLC#idAxqrAZbETBKqI8pg-1peVm8roBoqE5npeUBHz2DQw",
"https://mega.nz/file/H8t3HJaZ#9fNjShV7s7yXktxPv57aunyqf2xNHIfnN_RKPW6uVVY",
"https://mega.nz/file/r582lRCS#uLQOgfKehL9ETGMBrabkBXgSshOLyI7Ax5uULCiElMk",
"https://mega.nz/file/ip0HDTAB#j9jGPVgQJtMRQ9HAqZ_JUHNN7JxLC-Ro7XZIrP5tNxU",
"https://mega.nz/file/j8dWgbbQ#zShOVt0IVRN8nhGLrdgACWZa3OISrvRjNk8-1JF_tTs",
"https://mega.nz/file/vxNUnIaT#hUqzGv_iJYhEotg52aj68x3j2BsT0dkF036ywgtJaIA",
"https://mega.nz/file/bkF3kDjB#EaeGUiwgEpOU0CM9eGd-wcGaHF0kxTcGiJ00b0pMpCc",
"https://mega.nz/file/X8d0RJpT#wOUee4_QotOwiWxLiGPyXqnVQw1QRp9qaULTHFksv6w",
"https://mega.nz/file/nxtBHQQA#I9yMbshgnNDPUwvMpOc02HIYxdGmVOSJresr7ptiQlg",
"https://mega.nz/file/nssEnbwC#0YplVmsVpKo7zZAzkOHRFoaVDOxqbRgaMt5Hiar4UDk",
"https://mega.nz/file/v51WRZzJ#ApP9t3CKEKmN2GwdOPJkRPz95lXQ39xKllNxBvesXQs",
"https://mega.nz/file/Ht1UlQQB#N9muCH6B8_MqqMHGgLkpMRx0iZbM2HktZTIqhEP-TXc",
"https://mega.nz/file/6180mR7a#b7gXWCsomyZ2JG5Jq8crl0a7emlnabwMyefEuqOcq1g",
"https://mega.nz/file/zx1QUYza#kVwpShi4ugjkdZufmJYMsRciiObdHCBo0amIMaj5Aqs",
"https://mega.nz/file/S5EmTaCZ#pfK20r4plEuOn64Hyt3apZssqiLWsUzxnHaXw4TEZnw",
"https://mega.nz/file/ukljWJRY#wX6K0gLKQrVhzuJRQXGHR7Vgp-0RMTI2nL158ED71DQ",
"https://mega.nz/file/W9diDDjS#0GPP3d8N3_k_jc1cxF5QKU1A1ANx_B2wo8R1IQaK2nk",
"https://mega.nz/file/yokhRahK#CPkv6g0goLfHMLrc6uO0xtGMN4eYRG7QHjNDkGkdbag",
"https://mega.nz/file/zx8UmB7R#d1BU9Yd8tbbBBtdYiuPTWJt-fPUbQLm6qJCCl11dj80",
"https://mega.nz/file/2kswBYCS#ltpZH75ZJr4-EfJUgroyVk1a28ALP5Br4rVUYlXloLQ",
"https://mega.nz/file/vs1AXTKa#snWpaKPTkg_6j1ntb-8G43-3g7GNc7MxQRg_rM977rY",
"https://mega.nz/file/S4U21JiS#Z3iweMzPgFkc_tQmBUGWSCtAjp1RxRf13HJzHpGJKhM"


            };
            var audiobooks = await _context.Audiobook.AsNoTracking().ToListAsync();
            foreach (var item in audiobooks)
            {
                if (bookArray.Contains(item.Url))
                {
                    item.Error = false;
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    continue;
                }
            }
            return RedirectToAction(nameof(Index));
        }
         
    }
}
