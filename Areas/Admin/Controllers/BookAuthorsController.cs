using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Audiobooks.Data;
using Audiobooks.Models;
using Audiobooks.Services;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookAuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IAuthorService AuthorService { get; }

        public BookAuthorsController(ApplicationDbContext context,
            IAuthorService authorService)
        {
            _context = context;
            AuthorService = authorService;
        }

        // GET: Admin/BookAuthors
        public async Task<IActionResult> Index()
        {
            return View(await AuthorService.GetBookAuthors());
        }

        // GET: Admin/BookAuthors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BookAuthors == null)
            {
                return NotFound();
            }

            var bookAuthor = await AuthorService.GetBookAuthorById(id.Value);
            if (bookAuthor == null)
            {
                return NotFound();
            }

            return View(bookAuthor);
        }

        // GET: Admin/BookAuthors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BookAuthors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookAuthor bookAuthor)
        {
            if (ModelState.IsValid)
            {
                await AuthorService.AddBookAuthor(bookAuthor);
                return RedirectToAction(nameof(Index));
            }
            return View(bookAuthor);
        }

        // GET: Admin/BookAuthors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BookAuthors == null)
            {
                return NotFound();
            }

            var bookAuthor = await AuthorService.GetBookAuthorById(id.Value);
            if (bookAuthor == null)
            {
                return NotFound();
            }
            return View(bookAuthor);
        }

        // POST: Admin/BookAuthors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookAuthor bookAuthor)
        {
            if (id != bookAuthor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AuthorService.EditBookAuthor(bookAuthor);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookAuthorExists(bookAuthor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bookAuthor);
        }

        // GET: Admin/BookAuthors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BookAuthors == null)
            {
                return NotFound();
            }

            var bookAuthor = await AuthorService.GetBookAuthorById(id.Value);
            if (bookAuthor == null)
            {
                return NotFound();
            }

            return View(bookAuthor);
        }

        // POST: Admin/BookAuthors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BookAuthors == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BookAuthors'  is null.");
            }
            await AuthorService.DeleteBookAuthor(id);
            return RedirectToAction(nameof(Index));
        }

        private bool BookAuthorExists(int id)
        {
          return _context.BookAuthors.Any(e => e.Id == id);
        }
    }
}
