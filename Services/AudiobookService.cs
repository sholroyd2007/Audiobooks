using Audiobooks.Data;
using Audiobooks.Models;
using Audiobooks.ViewModels;
using CsvHelper;
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
        Task<IEnumerable<Audiobook>> GetBooksByAuthor(string author);
        Task<IEnumerable<Audiobook>> GetBooksByNarrator(string narrator);
        Task<IEnumerable<Audiobook>> GetBooksBySeries(string series);
        Task<IEnumerable<Audiobook>> GetBooksByCategoryId(int id);
        Task<IEnumerable<Audiobook>> GetSearchResults(string SearchTerm);
        Task<int> GetRandomBookId();
        Task<IEnumerable<Audiobook>> GetSortedBooks(string listType, string sort, string author = null, string narrator = null, string series = null, int? categoryId = null, string searchTerm = null);

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
        Task<Blurb> GetBlurbByAuthor(string author);
        Task<Blurb> GetBlurbBySeries(string series);
        Task<IEnumerable<Blurb>> GetBlurbs();
        Task<Blurb> GetBlurbById(int id);
        Task<Blurb> AddBlurb(Blurb blurb);
        Task<Blurb> EditBlurb(Blurb blurb);
        Task DeleteBlurb(int id);


        //Samples//
        Task<IEnumerable<Sample>> GetSamples();
        Task<Sample> GetSampleByAudiobookId(int id);
        Task<Sample> GetSampleById(int id);
        Task<Sample> AddSample(Sample sample);
        Task<Sample> EditSample(Sample sample);
        Task DeleteSample(int id);

        //XML//
        Task<DateTime> GetXmlLastModBookAuthor(string author);
        Task<DateTime> GetXmlLastModBookNarrator(string narrator);
        Task<DateTime> GetXmlLastModBookSeries(string series);
        Task<DateTime> GetXmlLastModBookCategory(int id);


        //Other//
        Task<IEnumerable<string>> GetAuthors();
        Task<IEnumerable<string>> GetNarrators();
        Task<IEnumerable<string>> GetBookSeries();
        int GetAuthorCount();
        int GetSeriesCount();
        int GetBookCount();
        int GetNarratorCount();
        Task<int> GetSeriesBookCount(string series);
        Task<int> GetAuthorBookCount(string author);
        Task<int> GetNarratorBookCount(string narrator);
        Task<int> GetCategoryBookCount(int id);
        Task ImportCatalogue(IFormFile file, IWebHostEnvironment hostingEnvironment);
        Task<AudiobookDetailViewModel> GetDetailPageViewModel(int id);
        Task<int> GetPreviousBookId(int id);
        Task<int> GetNextBookId(int id);
        Task<IEnumerable<int>> GetAllAudiobookIds();

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
                .Include(e => e.Category)
                .ToListAsync();
        }

        public async Task<Audiobook> GetAudiobookById(int id)
        {
            return await Context.Audiobook
                .AsNoTracking()
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public int GetAuthorCount()
        {
            return Context.Audiobook
                .AsNoTracking()
                .Where(a => a.Author != null)
                .ToList()
                .GroupBy(a => a.Author)
                .Distinct()
                .Count();
        }

        public async Task<int> GetAuthorBookCount(string author)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Where(a => a.Author == author)
                .Distinct()
                .ToListAsync();

            var authors = new List<string>();
            foreach (var book in books)
            {
                if (!authors.Contains(author))
                {
                    authors.Add(book.Author);
                }
            }
            return books.Count;
        }

        public async Task<IEnumerable<string>> GetAuthors()
        {
            var authors = await Context.Audiobook
                .AsNoTracking()
                .OrderBy(a => a.Author)
                .Select(a => a.Author)
                .Distinct()
                .ToListAsync();
            return authors;
        }

        public int GetBookCount()
        {
            return Context.Audiobook
                .AsNoTracking()
                .Count();
        }

        public async Task<IEnumerable<string>> GetBookSeries()
        {
            var series = await Context.Audiobook
                .AsNoTracking()
                .OrderBy(a => a.Series)
                .Select(a => a.Series)
                .Distinct()
                .ToListAsync();
            return series;
        }

        public async Task<IEnumerable<Audiobook>> GetBooksInSeries(int id)
        {
            var book = await GetAudiobookById(id);
            var seriesBooks = await Context.Audiobook
                .AsNoTracking()
                .Where(e => e.Series == book.Series)
                .OrderBy(e => e.SeriesNumber)
                .ToListAsync();
            return seriesBooks;
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            return await Context.Category
                .AsNoTracking()
                .ToListAsync();
        }

        public int GetNarratorCount()
        {
            return Context.Audiobook
                .AsNoTracking()
                .Where(a => a.Narrator != null)
                .ToList()
                .GroupBy(a => a.Narrator)
                .Distinct()
                .Count();
        }

        public async Task<int> GetNarratorBookCount(string narrator)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Where(a => a.Narrator == narrator)
                .Distinct()
                .ToListAsync();

            var narrators = new List<string>();
            foreach (var book in books)
            {
                if (!narrators.Contains(narrator))
                {
                    narrators.Add(book.Narrator);
                }
            }
            return books.Count;
        }

        public async Task<IEnumerable<string>> GetNarrators()
        {
            var narrators = await Context.Audiobook
                .AsNoTracking()
                .OrderBy(a => a.Narrator)
                .Select(a => a.Narrator)
                .Distinct()
                .ToListAsync();
            return narrators;
        }

        public async Task<IEnumerable<Audiobook>> GetRecentBooks()
        {
            return await Context.Audiobook
                .AsNoTracking()
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

        public async Task<Sample> GetSampleByAudiobookId(int id)
        {
            return await Context.Sample
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.AudiobookId == id);
        }

        public int GetSeriesCount()
        {
            return Context.Audiobook
                .AsNoTracking()
                .Where(a => a.Series != null)
                .ToList()
                .GroupBy(a => a.Series)
                .Distinct()
                .Count();
        }
        public async Task<int> GetSeriesBookCount(string seriesName)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Where(a => a.Series == seriesName)
                .Distinct()
                .ToListAsync();

            var series = new List<string>();
            foreach (var book in books)
            {
                if (!series.Contains(seriesName))
                {
                    series.Add(book.Series);
                }
            }
            return books.Count;
        }

        public async Task<DateTime> GetXmlLastModBookAuthor(string author)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Where(e => e.Author == author)
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

        public async Task<DateTime> GetXmlLastModBookNarrator(string narrator)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Where(e => e.Narrator == narrator)
                .ToListAsync();

            var latestBook = books.Max(e => e.DateAdded);
            return latestBook;
        }

        public async Task<DateTime> GetXmlLastModBookSeries(string series)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Where(e => e.Series == series)
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

            foreach (var audiobook in audiobooks)
            {
                Context.Audiobook.Remove(audiobook);
                await Context.SaveChangesAsync();
            }
        }

        public async Task<Audiobook> AddAudiobook(Audiobook audiobook)
        {
            audiobook.Author = audiobook.Author.Trim(' ');
            audiobook.Narrator = audiobook.Narrator.Trim(' ');
            audiobook.Name = audiobook.Name.Trim(' ');
            if (audiobook.Series != null)
            {
                audiobook.Series = audiobook.Series.Trim(' ');
            }
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

            await DeleteAllAudiobooks();
            
            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<AudiobookClassMap>();
                var records = csv.GetRecords<Audiobook>().ToList();
                foreach (var record in records)
                {
                    record.Series = String.IsNullOrWhiteSpace(record.Series) ? null : $"{record.Series}";
                    Context.Audiobook.Add(record);
                    await Context.SaveChangesAsync();

                    var category = await GetCategoryById(record.CategoryId);
                    record.CategoryId = category.Id;
                    Context.Audiobook.Update(record);
                    await Context.SaveChangesAsync();
                }

            }

            System.IO.File.Delete(fileName);
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

        public async Task<Sample> GetSampleById(int id)
        {
            var sample = await Context.Sample
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .FirstOrDefaultAsync(e => e.Id == id);
            return sample;
        }

        public async Task<Sample> AddSample(Sample sample)
        {
            Context.Add(sample);
            await Context.SaveChangesAsync();
            return sample;
        }

        public async Task<Sample> EditSample(Sample sample)
        {
            Context.Update(sample);
            await Context.SaveChangesAsync();
            return sample;
        }

        public async Task DeleteSample(int id)
        {
            var sample = await GetSampleById(id);
            Context.Remove(sample);
            await Context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Sample>> GetSamples()
        {
            var samples = await Context.Sample
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .ToListAsync();
            return samples;
        }

        public async Task<IEnumerable<Audiobook>> GetBooksByAuthor(string author)
        {
            return await Context.Audiobook
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.Author == author)
                .OrderByDescending(e => e.DateAdded)
                .ToListAsync();
        }

        public async Task<IEnumerable<Audiobook>> GetBooksByNarrator(string narrator)
        {
            return await Context.Audiobook
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.Narrator == narrator)
                .ToListAsync();
        }

        public async Task<IEnumerable<Audiobook>> GetBooksBySeries(string series)
        {
            return await Context.Audiobook
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.Series == series)
                .ToListAsync();
        }

        public async Task<AudiobookDetailViewModel> GetDetailPageViewModel(int id)
        {
            var audiobook = await GetAudiobookById(id);
            var vm = new AudiobookDetailViewModel()
            {
                Audiobook = audiobook,
                AuthorBooks = await GetBooksByAuthor(audiobook.Author),
                NarratorBooks = await GetBooksByNarrator(audiobook.Narrator),
                SeriesBooks = await GetBooksInSeries(audiobook.Id),
                Recommendation = await GetRecommendationByBookId(audiobook.Id),
                SeriesBlurb = (audiobook.Series != null ? await GetBlurbBySeries(audiobook.Series) : null),
                AuthorBlurb = await GetBlurbByAuthor(audiobook.Author),
                Sample = await GetSampleByAudiobookId(audiobook.Id)
            };
            return vm;
        }

        public async Task<IEnumerable<Audiobook>> GetBooksByCategoryId(int id)
        {
            var books = await Context.Audiobook
                .AsNoTracking()
                .Include(e=>e.Category)
                .Where(e => e.CategoryId == id)
                .ToListAsync();
            return books;

        }

        public async Task<IEnumerable<Audiobook>> GetSearchResults(string SearchTerm)
        {
            var results = await Context.Audiobook
                .Include(a => a.Category)
            .Where(a => a.Name.ToLower().Contains(SearchTerm.ToLower())
                    || a.Author.ToLower().Contains(SearchTerm.ToLower())
                    || a.Category.Name.ToLower().Contains(SearchTerm.ToLower())
                    || a.Series.ToLower().Contains(SearchTerm.ToLower())
                    || a.Narrator.ToLower().Contains(SearchTerm.ToLower()))
            .OrderByDescending(e => e.DateAdded)
            .ToListAsync();
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

        public async Task<Blurb> GetBlurbByAuthor(string author)
        {
            return await Context.Blurb
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.AuthorName == author);
        }

        public async Task<Blurb> GetBlurbBySeries(string series)
        {
            return await Context.Blurb
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.BookSeries == series);
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
            audiobook.Author = editedAudiobook.Author;
            audiobook.Narrator = editedAudiobook.Narrator;
            audiobook.CategoryId = editedAudiobook.CategoryId;
            audiobook.Url = editedAudiobook.Url;
            audiobook.ImageUrl = editedAudiobook.ImageUrl;
            audiobook.Length = editedAudiobook.Length;
            audiobook.Description = editedAudiobook.Description;
            if (!String.IsNullOrWhiteSpace(editedAudiobook.Series))
            {
                audiobook.SeriesNumber = editedAudiobook?.SeriesNumber;
                audiobook.Series = editedAudiobook?.Series;
            }
            Context.Update(audiobook);
            await Context.SaveChangesAsync();
            return audiobook;
        }

        public async Task<IEnumerable<Audiobook>> GetSortedBooks(string listType, string sort, string author = null, string narrator = null, string series = null, int? categoryId = null, string searchTerm = null)
        {
            var audiobooks = new List<Audiobook>();
            if (listType == "Browse")
            {
                var books = await GetAllAudiobooks();
                audiobooks.AddRange(books);
            }
            if (listType == "Author")
            {
                var books = await GetBooksByAuthor(author);
                audiobooks.AddRange(books);
            }
            if (listType == "Narrator")
            {
                var books = await GetBooksByNarrator(narrator);
                audiobooks.AddRange(books);
            }
            if (listType == "Series")
            {
                var books = await GetBooksBySeries(series);
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
            if (sort == "Author")
            {
                audiobooks = audiobooks.OrderBy(a => a.Author).ThenBy(e => e.Name).ToList();
            }
            if (sort == "Title")
            {
                audiobooks = audiobooks.OrderBy(a => a.Name).ToList();
            }
            if (sort == "Series")
            {
                audiobooks = audiobooks.OrderBy(a => a.SeriesNumber).ThenBy(e => e.Name).ToList();
            }
            if (sort == "Narrator")
            {
                audiobooks = audiobooks.OrderBy(a => a.Narrator).ThenBy(e => e.Name).ToList();
            }
            if (sort == "Category")
            {
                audiobooks = audiobooks.OrderBy(a => a.CategoryId).ThenBy(e=>e.Name).ToList();
            }

            return audiobooks;
        }
    }
}
