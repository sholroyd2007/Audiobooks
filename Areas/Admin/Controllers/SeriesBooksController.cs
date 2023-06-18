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
    public class SeriesBooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ISeriesService SeriesService { get; }

        public SeriesBooksController(ApplicationDbContext context,
            ISeriesService seriesService)
        {
            _context = context;
            SeriesService = seriesService;
        }

        // GET: Admin/SeriesBooks
        public async Task<IActionResult> Index()
        {
            return View(await SeriesService.GetSeriesBooks());
        }

        // GET: Admin/SeriesBooks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SeriesBooks == null)
            {
                return NotFound();
            }

            var seriesBook = await SeriesService.GetSeriesBookById(id.Value);
            if (seriesBook == null)
            {
                return NotFound();
            }

            return View(seriesBook);
        }

        // GET: Admin/SeriesBooks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/SeriesBooks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeriesBook seriesBook)
        {
            if (ModelState.IsValid)
            {
                await SeriesService.AddSeriesBook(seriesBook);
                return RedirectToAction(nameof(Index));
            }
            return View(seriesBook);
        }

        // GET: Admin/SeriesBooks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SeriesBooks == null)
            {
                return NotFound();
            }

            var seriesBook = await SeriesService.GetSeriesBookById(id.Value);
            if (seriesBook == null)
            {
                return NotFound();
            }
            return View(seriesBook);
        }

        // POST: Admin/SeriesBooks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SeriesBook seriesBook)
        {
            if (id != seriesBook.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await SeriesService.EditSeriesBook(seriesBook);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeriesBookExists(seriesBook.Id))
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
            return View(seriesBook);
        }

        // GET: Admin/SeriesBooks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.SeriesBooks == null)
            {
                return NotFound();
            }

            var seriesBook = await SeriesService.GetSeriesBookById(id.Value);
            if (seriesBook == null)
            {
                return NotFound();
            }

            return View(seriesBook);
        }

        // POST: Admin/SeriesBooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SeriesBooks == null)
            {
                return Problem("Entity set 'ApplicationDbContext.SeriesBooks'  is null.");
            }
            await SeriesService.DeleteSeriesBook(id);
            return RedirectToAction(nameof(Index));
        }

        private bool SeriesBookExists(int id)
        {
          return _context.SeriesBooks.Any(e => e.Id == id);
        }
    }
}
