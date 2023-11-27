using Audiobooks.Data;
using Audiobooks.Models;
using Audiobooks.ViewModels;
using CsvHelper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Services
{
    public interface IAudiobookService
    {

        //Audiobooks//
        Task<IEnumerable<Audiobook>> GetAllAudiobooks();
        Task<IEnumerable<Audiobook>> GetBooksInSeries(int id);
        Task<IEnumerable<Audiobook>> GetRecentBooks();
        Task<Audiobook> GetAudiobookById(int id);
        Task<Audiobook> AddAudiobook(Audiobook audiobook);
        Task<Audiobook> EditAudiobook(int id, Audiobook editedAudiobook);
        Task DeleteAudiobook(int id);
        Task DeleteAllAudiobooks();
        Task<IEnumerable<Audiobook>> GetBooksByAuthor(int authorId);
        Task<IEnumerable<Audiobook>> GetBooksByNarrator(int narratorId);
        Task<IEnumerable<Audiobook>> GetBooksBySeries(int seriesId);
        Task<IEnumerable<Audiobook>> GetBooksByCategoryId(int id);
        Task<IEnumerable<Audiobook>> GetSearchResults(string SearchTerm);
        Task<int> GetRandomBookId();
        Task<IEnumerable<Audiobook>> GetSortedBooks(string listType, string sort, Author author = null, Narrator narrator = null, Series series = null, int? categoryId = null, string searchTerm = null);

        //Categories//
        Task<IEnumerable<Category>> GetCategories();
        Task<Category> GetCategoryById(int id);
        Task<Category> AddCategory(Category category);
        Task<Category> EditCategory(Category category);
        Task DeleteCategory(int id);


        //Recommendations//
        Task<IEnumerable<Recommendation>> GetRecommendations();
        Task<Recommendation> GetRecommendationById(int id);
        Task<Recommendation> GetRecommendationByBookId(int id);
        Task<Recommendation> AddRecommendation(Recommendation recommendation);
        Task<Recommendation> EditRecommendation(Recommendation recommendation);
        Task DeleteRecommendation(int id);

        //Blurbs//
        Task<Blurb> GetBlurbByAuthor(int authorId);
        Task<Blurb> GetBlurbBySeries(int seriesId);
        Task<IEnumerable<Blurb>> GetBlurbs();
        Task<Blurb> GetBlurbById(int id);
        Task<Blurb> AddBlurb(Blurb blurb);
        Task<Blurb> EditBlurb(Blurb blurb);
        Task DeleteBlurb(int id);


        //XML//
        Task<DateTime> GetXmlLastModBookAuthor(int authorId);
        Task<DateTime> GetXmlLastModBookNarrator(int narratorId);
        Task<DateTime> GetXmlLastModBookSeries(int seriesId);
        Task<DateTime> GetXmlLastModBookCategory(int id);


        //Other//
        Task<IEnumerable<Author>> GetAuthors();
        Task<IEnumerable<Narrator>> GetNarrators();
        Task<IEnumerable<Series>> GetBookSeries();
        int GetAuthorCount();
        int GetSeriesCount();
        int GetBookCount();
        int GetNarratorCount();
        Task<int> GetSeriesBookCount(int seriesId);
        Task<int> GetAuthorBookCount(int authorId);
        Task<int> GetNarratorBookCount(int narratorId);
        Task<int> GetCategoryBookCount(int id);
        Task ImportCatalogue(IFormFile file, IWebHostEnvironment hostingEnvironment);
        Task UploadAuthorsAndNarrators(IFormFile file, IWebHostEnvironment hostingEnvironment);
        Task<AudiobookDetailViewModel> GetDetailPageViewModel(int id);
        Task<int> GetPreviousBookId(int id);
        Task<int> GetNextBookId(int id);
        Task<IEnumerable<int>> GetAllAudiobookIds();
        Task<Series> GetSeriesById(int id);
        Task<Author> GetAuthorById(int id);
        Task<Narrator> GetNarratorById(int id);
        Task<IEnumerable<Author>> GetAuthorsByBookId(int bookId);
        Task<IEnumerable<Narrator>> GetNarratorsByBookId(int bookId);
        Task<SeriesBook> GetSeriesBookByBookId(int bookId);
        Task<int> GetTotalDownloadCount();

        //ErrorReports
        Task<ErrorReport> GetErrorReportById(int id);
        Task<IEnumerable<ErrorReport>> GetAllErrorReports();
        Task<ErrorReport> EditErrorReport(int id, ErrorReport errorReport);
        Task<ErrorReport> AddErrorReport(ErrorReport errorReport);
        Task DeleteErrorReport(int id);


    }

    public class AudiobookService : IAudiobookService
    {
        private readonly Random _random = new Random();

        public AudiobookService(ApplicationDbContext context)
        {
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public async Task<IEnumerable<Audiobook>> GetAllAudiobooks()
        {
            return await Context.Audiobook
                .AsNoTracking()
                .Include(e=>e.Authors)
                .Include(e=>e.Narrators)
                .Include(e => e.Category)
                .ToListAsync();
        }

        public async Task<Audiobook> GetAudiobookById(int id)
        {
            return await Context.Audiobook
                .AsNoTracking()
                .Include(e=>e.Authors)
                .Include(e=>e.Narrators)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public int GetAuthorCount()
        {
            return Context.Authors.Count();
        }

        public async Task<int> GetAuthorBookCount(int authorId)
        {
            var books = await Context.BookAuthors.AsNoTracking().Where(e => e.AuthorId == authorId).ToListAsync();
            return books.Count();
        }

        public async Task<IEnumerable<Author>> GetAuthors()
        {
            var authors = await Context.BookAuthors.AsNoTracking().Select(e=>e.Author).Distinct().ToListAsync();
            authors = authors.OrderBy(e => e.Name).ToList();
            return authors;
        }

        public int GetBookCount()
        {
            return Context.Audiobook
                .AsNoTracking()
                .Count();
        }

        public async Task<IEnumerable<Series>> GetBookSeries()
        {
            var series = await Context.Series.AsNoTracking().ToListAsync();
            series = series.OrderBy(e => e.Name).ToList();
            return series;
        }

        public async Task<IEnumerable<Audiobook>> GetBooksInSeries(int id)
        {
            var seriesBook = await Context.SeriesBooks.AsNoTracking().Include(e => e.Series).FirstOrDefaultAsync(e => e.AudiobookId == id);
            var books = await Context.SeriesBooks
                .AsNoTracking()
                .Include(e=>e.Audiobook)
                .Where(e => e.SeriesId == seriesBook.SeriesId)
                .OrderBy(e => e.SeriesNumber)
                .Select(e=>e.Audiobook)
                .ToListAsync();
            return books;
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            var categories = await Context.Category
                .AsNoTracking()
                .ToListAsync();
            categories = categories.OrderBy(e => e.Name).ToList();
            return categories;
        }

        public int GetNarratorCount()
        {
            return Context.Narrators.AsNoTracking().Count();
        }

        public async Task<int> GetNarratorBookCount(int narratorId)
        {
            var books = await Context.BookNarrators
                .AsNoTracking()
                .Where(a => a.NarratorId == narratorId)
                .Distinct()
                .ToListAsync();

            return books.Count;
        }

        public async Task<IEnumerable<Narrator>> GetNarrators()
        {
            var narrators = await Context.BookNarrators.AsNoTracking().Select(e => e.Narrator).Distinct().ToListAsync();
            narrators = narrators.OrderBy(e => e.Name).ToList();
            return narrators;
        }

        public async Task<IEnumerable<Audiobook>> GetRecentBooks()
        {
            return await Context.Audiobook
                .AsNoTracking()
                .Include(e=>e.Authors)
                .OrderByDescending(a => a.DateAdded)
                .Take(10)
                .ToListAsync();
        }

        public async Task<Recommendation> GetRecommendationByBookId(int id)
        {
            return await Context.Recommendation
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.AudiobookId == id);
        }

        public async Task<IEnumerable<Recommendation>> GetRecommendations()
        {
            return await Context.Recommendation
                .AsNoTracking()
                .Include(a => a.audiobook)
                .ToListAsync();
        }

        public int GetSeriesCount()
        {
            return Context.Series.AsNoTracking().Count();
        }
        public async Task<int> GetSeriesBookCount(int seriesId)
        {
            var books = await Context.SeriesBooks
                .AsNoTracking()
                .Where(a => a.SeriesId == seriesId)
                .ToListAsync();

            return books.Count;
        }

        public async Task<DateTime> GetXmlLastModBookAuthor(int authorId)
        {
            var books = await Context.BookAuthors
                .AsNoTracking()
                .Include(e=>e.Audiobook)
                .Where(e => e.AuthorId == authorId)
                .Select(e=>e.Audiobook)
                .ToListAsync();

            var latestBook = books.Max(e => e.DateAdded);
            return latestBook;
        }

        public async Task<DateTime> GetXmlLastModBookCategory(int id)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Where(e => e.CategoryId == id)
                .ToListAsync();
            if (!books.Any())
            {
                return DateTime.UtcNow;
            }
            var latestBook = books.Max(e => e.DateAdded);
            return latestBook;

        }

        public async Task<DateTime> GetXmlLastModBookNarrator(int narratorId)
        {
            var books = await Context.BookNarrators
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .Where(e => e.NarratorId == narratorId)
                .Select(e => e.Audiobook)
                .ToListAsync();

            var latestBook = books.Max(e => e.DateAdded);
            return latestBook;
        }

        public async Task<DateTime> GetXmlLastModBookSeries(int seriesId)
        {
            var books = await Context.SeriesBooks
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .Where(e => e.SeriesId == seriesId)
                .Select(e => e.Audiobook)
                .ToListAsync();

            var latestBook = books.Max(e => e.DateAdded);
            return latestBook;
        }

        public async Task DeleteAllAudiobooks()
        {
            var audiobooks = await Context.Audiobook
                .AsNoTracking()
                .AsNoTracking()
                .ToListAsync();

            Context.Audiobook.RemoveRange(audiobooks);
            await Context.SaveChangesAsync();

        }

        public async Task<Audiobook> AddAudiobook(Audiobook audiobook)
        {
            //audiobook.Author = audiobook.Author.Trim(' ');
            //audiobook.Narrator = audiobook.Narrator.Trim(' ');
            audiobook.Name = audiobook.Name.Trim(' ');
            //if (audiobook.Series != null)
            //{
            //    audiobook.Series = audiobook.Series.Trim(' ');
            //}
            Context.Add(audiobook);
            await Context.SaveChangesAsync();
            return audiobook;
        }

        public async Task DeleteAudiobook(int id)
        {
            var audiobook = await GetAudiobookById(id);
            Context.Audiobook.Remove(audiobook);
            await Context.SaveChangesAsync();
        }

        public async Task ImportCatalogue(IFormFile file, IWebHostEnvironment hostingEnvironment)
        {

            string fileName = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
            using (FileStream fileStream = System.IO.File.Create(fileName))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }

            var existingBooks = await GetAllAudiobooks();

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<AudiobookClassMap>();
                var records = csv.GetRecords<Audiobook>().ToList();
                foreach (var record in records)
                {
                    if(!existingBooks.Any(e=>e.Name == record.Name))
                    {
                        Context.Audiobook.Add(record);
                        await Context.SaveChangesAsync();

                        var category = await GetCategoryById(record.CategoryId);
                        record.CategoryId = category.Id;
                        Context.Audiobook.Update(record);
                        await Context.SaveChangesAsync();
                    }
                }
            }

            System.IO.File.Delete(fileName);
        }

        public async Task UploadAuthorsAndNarrators(IFormFile file, IWebHostEnvironment hostingEnvironment)
        {
            if (file != null)
            {
                string filePath = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
                using (FileStream fileStream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var name = csv.GetField("Name");
                        var book = await Context.Audiobook
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.Name == name);
                        if (book != null)
                        {
                            var authors = csv.GetField("Authors");
                            var narrators = csv.GetField("Narrators");
                            var series = csv.GetField("Series");

                            //Authors
                            if (!String.IsNullOrWhiteSpace(authors))
                            {
                                var allAuthors = await Context.Authors.ToListAsync();
                                var allBookAuthors = await Context.BookAuthors.Where(e=>e.AudiobookId == book.Id).ToListAsync();

                                var aList = authors.Split(", ");
                                foreach (var item in aList)
                                {
                                    //Check if Question Tag already exists
                                    if (!allBookAuthors.Any(e => e.Name == item))
                                    {
                                        //Check if tag already exists
                                        if (!allAuthors.Any(e => e.Name == item))
                                        {
                                            //Create QuestionTag from new Tag
                                            //Create Tag
                                            var author = new Author();
                                            author.Name = item.Trim();
                                            Context.Authors.Add(author);
                                            await Context.SaveChangesAsync();

                                            //Create Question Tag
                                            var bookAuthor = new BookAuthor();
                                            bookAuthor.AudiobookId = book.Id;
                                            bookAuthor.AuthorId = author.Id;
                                            Context.BookAuthors.Add(bookAuthor);
                                            await Context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            //Create QuestionTag from Existing Tag
                                            //Find Tag
                                            var author = await Context.Authors.FirstOrDefaultAsync(e => e.Name == item);

                                            //Create Question Tag
                                            var bookAuthor = new BookAuthor();
                                            bookAuthor.AudiobookId = book.Id;
                                            bookAuthor.AuthorId = author.Id;
                                            Context.BookAuthors.Add(bookAuthor);
                                            await Context.SaveChangesAsync();
                                        }
                                    }

                                }
                            }

                            //Narrators
                            if (!String.IsNullOrWhiteSpace(narrators))
                            {
                                var allNarrators = await Context.Narrators.ToListAsync();
                                var allBookNarrators = await Context.BookNarrators.Where(e => e.AudiobookId == book.Id).ToListAsync();

                                var nList = narrators.Split(", ");
                                foreach (var item in nList)
                                {
                                    //Check if Question Tag already exists
                                    if (!allBookNarrators.Any(e => e.Name == item))
                                    {
                                        //Check if tag already exists
                                        if (!allNarrators.Any(e => e.Name == item))
                                        {
                                            //Create QuestionTag from new Tag
                                            //Create Tag
                                            var narrator = new Narrator();
                                            narrator.Name = item.Trim();
                                            Context.Narrators.Add(narrator);
                                            await Context.SaveChangesAsync();

                                            //Create Question Tag
                                            var bookNarrator = new BookNarrator();
                                            bookNarrator.AudiobookId = book.Id;
                                            bookNarrator.NarratorId = narrator.Id;
                                            Context.BookNarrators.Add(bookNarrator);
                                            await Context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            //Create QuestionTag from Existing Tag
                                            //Find Tag
                                            var narrator = await Context.Narrators.FirstOrDefaultAsync(e => e.Name == item);

                                            //Create Question Tag
                                            var bookNarrator = new BookNarrator();
                                            bookNarrator.AudiobookId = book.Id;
                                            bookNarrator.NarratorId = narrator.Id;
                                            Context.BookNarrators.Add(bookNarrator);
                                            await Context.SaveChangesAsync();
                                        }
                                    }

                                }
                            }

                            //Series
                            if (!String.IsNullOrWhiteSpace(series))
                            {
                                var allSeries = await Context.Series.ToListAsync();
                                var allSeriesBooks = await Context.SeriesBooks.Where(e => e.AudiobookId == book.Id).ToListAsync();

                                var sList = series.Split(", ");
                                foreach (var item in sList)
                                {
                                    //Check if Question Tag already exists
                                    if (!allSeriesBooks.Any(e => e.Name == item))
                                    {
                                        //Check if tag already exists
                                        if (!allSeries.Any(e => e.Name == item))
                                        {
                                            //Create QuestionTag from new Tag
                                            //Create Tag
                                            var s = new Series();
                                            s.Name = item.Trim();
                                            Context.Series.Add(s);
                                            await Context.SaveChangesAsync();

                                            //Create Question Tag
                                            var seriesNumber = csv.GetField("SeriesNumber");
                                            var seriesBook = new SeriesBook();
                                            seriesBook.AudiobookId = book.Id;
                                            seriesBook.SeriesId = s.Id;
                                            if(seriesNumber != null)
                                            {
                                                seriesBook.SeriesNumber = Decimal.Parse(seriesNumber);
                                            }
                                            
                                            Context.SeriesBooks.Add(seriesBook);
                                            await Context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            //Create QuestionTag from Existing Tag
                                            //Find Tag
                                            var s = await Context.Series.FirstOrDefaultAsync(e => e.Name == item);

                                            //Create Question Tag
                                            var seriesNumber = csv.GetField("SeriesNumber");
                                            var seriesBook = new SeriesBook();
                                            seriesBook.AudiobookId = book.Id;
                                            seriesBook.SeriesId = s.Id;
                                            if (seriesNumber != null)
                                            {
                                                seriesBook.SeriesNumber = Decimal.Parse(seriesNumber);
                                            }

                                            Context.SeriesBooks.Add(seriesBook);
                                            await Context.SaveChangesAsync();
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                System.IO.File.Delete(filePath);
            }
        }

        public async Task<Category> GetCategoryById(int id)
        {
            var category = await Context.Category
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return category;
        }

        public async Task<Category> AddCategory(Category category)
        {
            Context.Add(category);
            await Context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> EditCategory(Category category)
        {
            Context.Update(category);
            await Context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteCategory(int id)
        {
            var category = await GetCategoryById(id);
            Context.Remove(category);
            await Context.SaveChangesAsync();
        }

        public async Task<Recommendation> AddRecommendation(Recommendation recommendation)
        {
            Context.Add(recommendation);
            await Context.SaveChangesAsync();
            return recommendation;
        }

        public async Task<Recommendation> EditRecommendation(Recommendation recommendation)
        {
            Context.Update(recommendation);
            await Context.SaveChangesAsync();
            return recommendation;
        }

        public async Task DeleteRecommendation(int id)
        {
            var recommendation = await GetRecommendationById(id);
            Context.Remove(recommendation);
            await Context.SaveChangesAsync();
        }

        public async Task<Recommendation> GetRecommendationById(int id)
        {
            var recommendation = await Context.Recommendation
                .AsNoTracking()
                .Include(e => e.audiobook)
                .FirstOrDefaultAsync(e => e.Id == id);
            return recommendation;
        }

        public async Task<IEnumerable<Audiobook>> GetBooksByAuthor(int authorId)
        {
            var author = await GetAuthorById(authorId);
            var bookAuthors = await Context.BookAuthors
                .AsNoTracking()
                .Where(e=>e.AuthorId == author.Id)
                .ToListAsync();
            var books = new List<Audiobook>();  
            foreach (var item in bookAuthors)
            {
                var audiobook = await GetAudiobookById(item.AudiobookId);
                books.Add(audiobook);
            }

            //return await Context.BookAuthors
            //    .AsNoTracking()
            //    .Include(e => e.Audiobook)
            //    .ThenInclude(e => e.Category)
            //    .Include(e=>e.Audiobook.Authors)
            //    .Include(e=>e.Audiobook.Narrators)
            //    .Where(e => e.AuthorId == authorId)
            //    .Select(e=>e.Audiobook)
            //    .OrderByDescending(e => e.DateAdded)
            //    .ToListAsync();

            return books;
        }

        public async Task<IEnumerable<Audiobook>> GetBooksByNarrator(int narratorId)
        {
            return await Context.BookNarrators
                .AsNoTracking()
                .Include(e=>e.Audiobook)
                .ThenInclude(e => e.Category)
                .Where(e => e.NarratorId == narratorId)
                .Select(e=>e.Audiobook)
                .ToListAsync();
        }

        public async Task<IEnumerable<Audiobook>> GetBooksBySeries(int seriesId)
        {
            return await Context.SeriesBooks
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .ThenInclude(e=>e.Category)
                .Where(e => e.SeriesId == seriesId)
                .OrderBy(e=>e.SeriesNumber)
                .Select(e=>e.Audiobook)
                .ToListAsync();
        }

        public async Task<AudiobookDetailViewModel> GetDetailPageViewModel(int id)
        {
            var audiobook = await GetAudiobookById(id);
            var vm = new AudiobookDetailViewModel()
            {
                Audiobook = audiobook,
                //AuthorBooks = await GetBooksByAuthor(audiobook.Author),
                //NarratorBooks = await GetBooksByNarrator(audiobook.Narrator),
                SeriesBooks = await GetBooksInSeries(audiobook.Id),
                Recommendation = await GetRecommendationByBookId(audiobook.Id),
                //SeriesBlurb = (audiobook.Series != null ? await GetBlurbBySeries(audiobook.Series) : null),
                //AuthorBlurb = await GetBlurbByAuthor(audiobook.Author),
            };
            return vm;
        }

        public async Task<IEnumerable<Audiobook>> GetBooksByCategoryId(int id)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.CategoryId == id)
                .ToListAsync();
            return books;

        }

        public async Task<IEnumerable<Audiobook>> GetSearchResults(string SearchTerm)
        {
            var results = new List<Audiobook>();

            //Search Audiobook and Categories
            var nameResults = await Context.Audiobook
                .Include(a => a.Category)
            .Where(a => a.Name.ToLower().Contains(SearchTerm.ToLower())
                    || a.Category.Name.ToLower().Contains(SearchTerm.ToLower()))
            .OrderByDescending(e => e.DateAdded)
            .ToListAsync();
            results.AddRange(nameResults);

            //Search Authors
            var authorResults = await Context.BookAuthors
                .AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Audiobook)
                .ThenInclude(e => e.Category)
                .Where(e => e.Author.Name.ToLower().Contains(SearchTerm.ToLower()))
                .Select(e => e.Audiobook)
                .ToListAsync();
            results.AddRange(authorResults);

            //Search Narrators
            var narratorResults = await Context.BookNarrators
                .AsNoTracking()
                .Include(a => a.Narrator)
                .Include(a => a.Audiobook)
                .ThenInclude(e=>e.Category)
                .Where(e => e.Narrator.Name.ToLower().Contains(SearchTerm.ToLower()))
                .Select(e => e.Audiobook)
                .ToListAsync();
            results.AddRange(narratorResults);

            //Search Series
            var seriesResults = await Context.SeriesBooks
                .AsNoTracking()
                .Include(a => a.Series)
                .Include(a => a.Audiobook)
                .ThenInclude(e => e.Category)
                .Where(e => e.Series.Name.ToLower().Contains(SearchTerm.ToLower()))
                .Select(e => e.Audiobook)
                .ToListAsync();
            results.AddRange(seriesResults);

            results = results.DistinctBy(e=>e.Id).ToList();

            return results;
        }

        public async Task<int> GetCategoryBookCount(int id)
        {
            var category = await GetCategoryById(id);
            if (category != null)
            {
                var books = await GetBooksByCategoryId(category.Id);
                return books.Count();
            }

            return 0;
        }

        public async Task<int> GetRandomBookId()
        {
            var books = await GetAllAudiobooks();
            var ids = books.Select(a => a.Id).ToArray();
            var id = ids[_random.Next(1, ids.Length)];
            return id;
        }

        public async Task<Blurb> GetBlurbByAuthor(int authorId)
        {
            var author = await Context.Authors.AsNoTracking().FirstOrDefaultAsync(e => e.Id == authorId);
            return await Context.Blurb
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.AuthorName == author.Name);
        }

        public async Task<Blurb> GetBlurbBySeries(int seriesId)
        {
            var series = await Context.Series.AsNoTracking().FirstOrDefaultAsync(e => e.Id == seriesId);
            return await Context.Blurb
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.BookSeries == series.Name);
        }

        public async Task<IEnumerable<Blurb>> GetBlurbs()
        {
            var blurbs = await Context.Blurb
                .AsNoTracking()
                .ToListAsync();
            return blurbs;
        }

        public async Task<Blurb> GetBlurbById(int id)
        {
            var blurb = await Context.Blurb
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return blurb;
        }

        public async Task<Blurb> AddBlurb(Blurb blurb)
        {
            Context.Add(blurb);
            await Context.SaveChangesAsync();
            return blurb;
        }

        public async Task<Blurb> EditBlurb(Blurb blurb)
        {
            Context.Update(blurb);
            await Context.SaveChangesAsync();
            return blurb;
        }

        public async Task DeleteBlurb(int id)
        {
            var blurb = await GetBlurbById(id);
            Context.Remove(blurb);
            await Context.SaveChangesAsync();
        }

        public async Task<int> GetPreviousBookId(int id)
        {
            var allIds = await GetAllAudiobookIds();
            allIds = allIds.Reverse();
            var newBookId = allIds.FirstOrDefault(e => e < id);
            return newBookId;

            //var allbooks = await GetAllAudiobooks();
            //allbooks = allbooks.Reverse();
            //var newBook = allbooks.FirstOrDefault(e => e.Id < id);
            //return newBook.Id;
        }

        public async Task<int> GetNextBookId(int id)
        {
            var allIds = await GetAllAudiobookIds();
            var newBookId = allIds.FirstOrDefault(e => e > id);
            return newBookId;

            //var allbooks = await GetAllAudiobooks();
            //var newBook = allbooks.FirstOrDefault(e => e.Id > id);
            //return newBook.Id;
        }

        public async Task<IEnumerable<int>> GetAllAudiobookIds()
        {
            var ids = await Context.Audiobook.AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).ToListAsync();
            return ids;
        }

        public async Task<Audiobook> EditAudiobook(int id, Audiobook editedAudiobook)
        {
            var audiobook = await GetAudiobookById(id);
            audiobook.Name = editedAudiobook.Name;
            //audiobook.Author = editedAudiobook.Author;
            //audiobook.Narrator = editedAudiobook.Narrator;
            audiobook.CategoryId = editedAudiobook.CategoryId;
            audiobook.Url = editedAudiobook.Url;
            audiobook.ImageUrl = editedAudiobook.ImageUrl;
            audiobook.Length = editedAudiobook.Length;
            audiobook.Description = editedAudiobook.Description;
            audiobook.Error = editedAudiobook.Error;
            //if (!String.IsNullOrWhiteSpace(editedAudiobook.Series))
            //{
            //    audiobook.SeriesNumber = editedAudiobook?.SeriesNumber;
            //    audiobook.Series = editedAudiobook?.Series;
            //}
            Context.Update(audiobook);
            await Context.SaveChangesAsync();
            return audiobook;
        }

        public async Task<IEnumerable<Audiobook>> GetSortedBooks(string listType, string sort, Author author = null, Narrator narrator = null, Series series = null, int? categoryId = null, string searchTerm = null)
        {
            var audiobooks = new List<Audiobook>();
            if (listType == "Browse")
            {
                var books = await GetAllAudiobooks();
                audiobooks.AddRange(books);
            }
            if (listType == "Author")
            {
                var books = await GetBooksByAuthor(author.Id);
                audiobooks.AddRange(books);
            }
            if (listType == "Narrator")
            {
                var books = await GetBooksByNarrator(narrator.Id);
                audiobooks.AddRange(books);
            }
            if (listType == "Series")
            {
                var books = await GetBooksBySeries(series.Id);
                audiobooks.AddRange(books);
            }
            if (listType == "Category")
            {
                var books = await GetBooksByCategoryId(categoryId.Value);
                audiobooks.AddRange(books);
            }
            if (listType == "Search")
            {
                var books = await GetSearchResults(searchTerm);
                audiobooks.AddRange(books);
            }

            if (sort == null || sort == "Recent")
            {
                audiobooks = audiobooks.OrderByDescending(a => a.DateAdded).ThenBy(e => e.Name).ToList();
            }
            if (sort == "Title")
            {
                audiobooks = audiobooks.OrderBy(a => a.Name).ToList();
            }
            return audiobooks;
        }

        public async Task<Series> GetSeriesById(int id)
        {
            var series = await Context.Series
                .AsNoTracking()
                .Include(e=>e.Books)
                .FirstOrDefaultAsync(e => e.Id == id);
            return series;
        }

        public async Task<Author> GetAuthorById(int id)
        {
            var author = await Context.Authors
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return author;
        }

        public async Task<Narrator> GetNarratorById(int id)
        {
            var narrator = await Context.Narrators
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return narrator;
        }

        public async Task<IEnumerable<Author>> GetAuthorsByBookId(int bookId)
        {
            var book  = await GetAudiobookById(bookId);
            var authors = await Context.BookAuthors
                .AsNoTracking()
                .Include(e => e.Author)
                .Where(e => e.AudiobookId == bookId)
                .Select(e => e.Author)
                .ToListAsync();
            return authors;
        }

        public async Task<IEnumerable<Narrator>> GetNarratorsByBookId(int bookId)
        {
            var book = await GetAudiobookById(bookId);
            var narrators = await Context.BookNarrators
                .AsNoTracking()
                .Include(e => e.Narrator)
                .Where(e => e.AudiobookId == bookId)
                .Select(e => e.Narrator)
                .ToListAsync();
            return narrators;
        }

        public async Task<SeriesBook> GetSeriesBookByBookId(int bookId)
        {
            var book = await GetAudiobookById(bookId);
            var seriesBook = await Context.SeriesBooks
                .AsNoTracking()
                .Include(e => e.Series)
                .FirstOrDefaultAsync(e => e.AudiobookId == bookId);
            return seriesBook;
        }

        public async Task<ErrorReport> GetErrorReportById(int id)
        {
            var errorReport = await Context.ErrorReports.Include(e=>e.Audiobook).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return errorReport;
        }

        public async Task<IEnumerable<ErrorReport>> GetAllErrorReports()
        {
            var errorReports = await Context.ErrorReports.Include(e => e.Audiobook).AsNoTracking().ToListAsync();
            return errorReports;
        }

        public async Task<ErrorReport> EditErrorReport(int id, ErrorReport errorReport)
        {
            var report = await GetErrorReportById(id);
            if (report != null)
            {
                report.ErrorStatus = errorReport.ErrorStatus;
                report.AudiobookId = report.AudiobookId;
            }
            Context.Update(report);
            await Context.SaveChangesAsync();
            return errorReport;
        }

        public async Task<ErrorReport> AddErrorReport(ErrorReport errorReport)
        {
            Context.Add(errorReport);
            await Context.SaveChangesAsync();
            return errorReport;
        }

        public async Task DeleteErrorReport(int id)
        {
            var errorReport = await GetErrorReportById(id);
            Context.Remove(errorReport);
            await Context.SaveChangesAsync();
        }

        public async Task<int> GetTotalDownloadCount()
        {
            
            var audiobooks = await GetAllAudiobooks();
            var result = audiobooks.Sum(e => e.Downloads);
            return result;
        }
    }
}
