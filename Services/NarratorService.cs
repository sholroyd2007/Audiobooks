using Audiobooks.Data;
using Audiobooks.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiobooks.Services
{
    public interface INarratorService
    {
        //Narrator
        Task<IEnumerable<Narrator>> GetNarrators();
        Task<Narrator> GetNarratorById(int id);
        Task<Narrator> AddNarrator(Narrator narrator);
        Task<Narrator> EditNarrator(Narrator narrator);
        Task DeleteNarrator(int id);

        //BookNarrator
        Task<IEnumerable<BookNarrator>> GetBookNarrators();
        Task<BookNarrator> GetBookNarratorById(int id);
        Task<BookNarrator> AddBookNarrator(BookNarrator bookNarrator);
        Task<BookNarrator> EditBookNarrator(BookNarrator bookNarrator);
        Task DeleteBookNarrator(int id);
    }
    public class NarratorService : INarratorService
    {
        public NarratorService(ApplicationDbContext context)
        {
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public async Task<Narrator> AddNarrator(Narrator narrator)
        {
            Context.Add(narrator);
            await Context.SaveChangesAsync();
            return narrator;
        }

        public async Task<BookNarrator> AddBookNarrator(BookNarrator bookNarrator)
        {
            Context.Add(bookNarrator);
            await Context.SaveChangesAsync();
            return bookNarrator;
        }

        public async Task DeleteNarrator(int id)
        {
            var narrator = await GetNarratorById(id);
            Context.Remove(narrator);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteBookNarrator(int id)
        {
            var bookNarrator = await GetBookNarratorById(id);
            Context.Remove(bookNarrator);
            await Context.SaveChangesAsync();
        }

        public async Task<Narrator> EditNarrator(Narrator narrator)
        {
            Context.Update(narrator);
            await Context.SaveChangesAsync();
            return narrator;
        }

        public async Task<BookNarrator> EditBookNarrator(BookNarrator bookNarrator)
        {
            Context.Update(bookNarrator);
            await Context.SaveChangesAsync();
            return bookNarrator;
        }

        public async Task<Narrator> GetNarratorById(int id)
        {
            var narrator = await Context.Narrators.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return narrator;
        }

        public async Task<IEnumerable<Narrator>> GetNarrators()
        {
            var narrators = await Context.Narrators.AsNoTracking().ToListAsync();
            return narrators;
        }

        public async Task<BookNarrator> GetBookNarratorById(int id)
        {
            var bookNarrator = await Context.BookNarrators
                .AsNoTracking()
                .Include(e=>e.Audiobook)
                .ThenInclude(e=>e.Category)
                .Include(e=>e.Narrator)
                .FirstOrDefaultAsync(e => e.Id == id);
            return bookNarrator;
        }

        public async Task<IEnumerable<BookNarrator>> GetBookNarrators()
        {
            var bookNarrators = await Context.BookNarrators
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .ThenInclude(e => e.Category)
                .Include(e => e.Narrator)
                .ToListAsync();
            return bookNarrators;
        }

       
    }
}
