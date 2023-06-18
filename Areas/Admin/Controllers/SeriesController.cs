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
    public class SeriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ISeriesService SeriesService { get; }

        public SeriesController(ApplicationDbContext context,
            ISeriesService seriesService)
        {
            _context = context;
            SeriesService = seriesService;
        }

        // GET: Admin/Series
        public async Task<IActionResult> Index()
        {
              return View(await SeriesService.GetSeries());
        }

        // GET: Admin/Series/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Series == null)
            {
                return NotFound();
            }

            var series = await SeriesService.GetSeriesById(id.Value);
            if (series == null)
            {
                return NotFound();
            }

            return View(series);
        }

        // GET: Admin/Series/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Series/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Series series)
        {
            if (ModelState.IsValid)
            {
                await SeriesService.AddSeries(series);
                return RedirectToAction(nameof(Index));
            }
            return View(series);
        }

        // GET: Admin/Series/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Series == null)
            {
                return NotFound();
            }

            var series = await SeriesService.GetSeriesById(id.Value);
            if (series == null)
            {
                return NotFound();
            }
            return View(series);
        }

        // POST: Admin/Series/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Series series)
        {
            if (id != series.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await SeriesService.EditSeries(series);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeriesExists(series.Id))
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
            return View(series);
        }

        // GET: Admin/Series/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Series == null)
            {
                return NotFound();
            }

            var series = await SeriesService.GetSeriesById(id.Value);
            if (series == null)
            {
                return NotFound();
            }

            return View(series);
        }

        // POST: Admin/Series/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Series == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Series'  is null.");
            }
            await SeriesService.DeleteSeries(id);
            return RedirectToAction(nameof(Index));
        }

        private bool SeriesExists(int id)
        {
          return _context.Series.Any(e => e.Id == id);
        }
    }
}
