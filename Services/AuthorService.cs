
using Audiobooks.Data;
using Audiobooks.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiobooks.Services
{
    public interface IAuthorService
    {
        //Author
        Task<IEnumerable<Author>> GetAuthors();
        Task<Author> GetAuthorById(int id);
        Task<Author> AddAuthor(Author author);
        Task<Author> EditAuthor(Author author);
        Task DeleteAuthor(int id);

        //BookAuthor
        Task<IEnumerable<BookAuthor>> GetBookAuthors();
        Task<BookAuthor> GetBookAuthorById(int id);
        Task<BookAuthor> AddBookAuthor(BookAuthor bookAuthor);
        Task<BookAuthor> EditBookAuthor(BookAuthor bookAuthor);
        Task DeleteBookAuthor(int id);
    }
    public class AuthorService : IAuthorService
    {
        public AuthorService(ApplicationDbContext context)
        {
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public async Task<Author> AddAuthor(Author author)
        {
            Context.Add(author);
            await Context.SaveChangesAsync();
            return author;
        }

        public async Task<BookAuthor> AddBookAuthor(BookAuthor bookAuthor)
        {
            Context.Add(bookAuthor);
            await Context.SaveChangesAsync();
            return bookAuthor;
        }

        public async Task DeleteAuthor(int id)
        {
            var author = await GetAuthorById(id);
            Context.Remove(author);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteBookAuthor(int id)
        {
            var bookAuthor = await GetBookAuthorById(id);
            Context.Remove(bookAuthor);
            await Context.SaveChangesAsync();
        }

        public async Task<Author> EditAuthor(Author author)
        {
            Context.Update(author);
            await Context.SaveChangesAsync();
            return author;
        }

        public async Task<BookAuthor> EditBookAuthor(BookAuthor bookAuthor)
        {
            Context.Update(bookAuthor);
            await Context.SaveChangesAsync();   
            return bookAuthor;
        }

        public async Task<Author> GetAuthorById(int id)
        {
            var author = await Context.Authors.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return author;
        }

        public async Task<IEnumerable<Author>> GetAuthors()
        {
            var authors = await Context.Authors.AsNoTracking().ToListAsync();
            return authors;
        }

        public async Task<BookAuthor> GetBookAuthorById(int id)
        {
            var bookAuthor = await Context.BookAuthors
                .AsNoTracking()
                .Include(e => e.Audiobook)
                .ThenInclude(e => e.Category)
                .Include(e => e.Author)
                .FirstOrDefaultAsync(e => e.Id == id); 
            return bookAuthor;
        }

        public async Task<IEnumerable<BookAuthor>> GetBookAuthors()
        {
            var bookAuthors = await Context.BookAuthors
                .AsNoTracking()
                .Include(e=>e.Audiobook)
                .ThenInclude(e=>e.Category)
                .Include(e=>e.Author)
                .ToListAsync();
            return bookAuthors;
        }
    }
}
