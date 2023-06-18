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
    public class BookNarratorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public INarratorService NarratorService { get; }

        public BookNarratorsController(ApplicationDbContext context,
            INarratorService narratorService)
        {
            _context = context;
            NarratorService = narratorService;
        }

        // GET: Admin/BookNarrators
        public async Task<IActionResult> Index()
        {
            return View(await NarratorService.GetBookNarrators());
        }

        // GET: Admin/BookNarrators/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BookNarrators == null)
            {
                return NotFound();
            }

            var bookNarrator = await NarratorService.GetBookNarratorById(id.Value);
            if (bookNarrator == null)
            {
                return NotFound();
            }

            return View(bookNarrator);
        }

        // GET: Admin/BookNarrators/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BookNarrators/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookNarrator bookNarrator)
        {
            if (ModelState.IsValid)
            {
                await NarratorService.AddBookNarrator(bookNarrator);
                return RedirectToAction(nameof(Index));
            }
            return View(bookNarrator);
        }

        // GET: Admin/BookNarrators/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BookNarrators == null)
            {
                return NotFound();
            }

            var bookNarrator = await NarratorService.GetBookNarratorById(id.Value);
            if (bookNarrator == null)
            {
                return NotFound();
            }
            return View(bookNarrator);
        }

        // POST: Admin/BookNarrators/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookNarrator bookNarrator)
        {
            if (id != bookNarrator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await NarratorService.EditBookNarrator(bookNarrator);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookNarratorExists(bookNarrator.Id))
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
            return View(bookNarrator);
        }

        // GET: Admin/BookNarrators/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BookNarrators == null)
            {
                return NotFound();
            }

            var bookNarrator = await NarratorService.GetBookNarratorById(id.Value);
            if (bookNarrator == null)
            {
                return NotFound();
            }

            return View(bookNarrator);
        }

        // POST: Admin/BookNarrators/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BookNarrators == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BookNarrators'  is null.");
            }
            await NarratorService.DeleteBookNarrator(id);
            return RedirectToAction(nameof(Index));
        }

        private bool BookNarratorExists(int id)
        {
          return _context.BookNarrators.Any(e => e.Id == id);
        }
    }
}
