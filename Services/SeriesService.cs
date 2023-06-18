using Audiobooks.Data;
using Audiobooks.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiobooks.Services
{
    public interface ISeriesService
    {
        //Series
        Task<IEnumerable<Series>> GetSeries();
        Task<Series> GetSeriesById(int id);
        Task<Series> AddSeries(Series series);
        Task<Series> EditSeries(Series series);
        Task DeleteSeries(int id);

        //SeriesBook
        Task<IEnumerable<SeriesBook>> GetSeriesBooks();
        Task<SeriesBook> GetSeriesBookById(int id);
        Task<SeriesBook> AddSeriesBook(SeriesBook seriesBook);
        Task<SeriesBook> EditSeriesBook(SeriesBook seriesBook);
        Task DeleteSeriesBook(int id);
    }
    public class SeriesService : ISeriesService
    {
        public SeriesService(ApplicationDbContext context)
        {
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public async Task<Series> AddSeries(Series narrator)
        {
            Context.Add(narrator);
            await Context.SaveChangesAsync();
            return narrator;
        }

        public async Task<SeriesBook> AddSeriesBook(SeriesBook bookSeries)
        {
            Context.Add(bookSeries);
            await Context.SaveChangesAsync();
            return bookSeries;
        }

        public async Task DeleteSeries(int id)
        {
            var narrator = await GetSeriesById(id);
            Context.Remove(narrator);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteSeriesBook(int id)
        {
            var bookSeries = await GetSeriesBookById(id);
            Context.Remove(bookSeries);
            await Context.SaveChangesAsync();
        }

        public async Task<Series> EditSeries(Series narrator)
        {
            Context.Update(narrator);
            await Context.SaveChangesAsync();
            return narrator;
        }

        public async Task<SeriesBook> EditSeriesBook(SeriesBook bookSeries)
        {
            Context.Update(bookSeries);
            await Context.SaveChangesAsync();
            return bookSeries;
        }

        public async Task<Series> GetSeriesById(int id)
        {
            var narrator = await Context.Series.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return narrator;
        }

        public async Task<IEnumerable<Series>> GetSeries()
        {
            var narrators = await Context.Series.AsNoTracking().ToListAsync();
            return narrators;
        }

        public async Task<SeriesBook> GetSeriesBookById(int id)
        {
            var bookSeries = await Context.SeriesBooks
                .AsNoTracking()
                .Include(e=>e.Audiobook)
                .ThenInclude(e=>e.Category)
                .Include(e=>e.Series)
                .FirstOrDefaultAsync(e => e.Id == id);
            return bookSeries;
        }

        public async Task<IEnumerable<SeriesBook>> GetSeriesBooks()
        {
            var bookSeriess = await Context.SeriesBooks
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .ThenInclude(e => e.Category)
                .Include(e => e.Series)
                .ToListAsync();
            return bookSeriess;
        }
    }
}
