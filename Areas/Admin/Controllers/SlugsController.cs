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

namespace Audiobooks.Areas.Admin
{
    [Area("Admin")]
    public class SlugsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ISlugService SlugService { get; }

        public SlugsController(ApplicationDbContext context,
            ISlugService slugService)
        {
            _context = context;
            SlugService = slugService;
        }

        // GET: Admin/Slugs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Slugs.ToListAsync());
        }

        // GET: Admin/Slugs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slug = await _context.Slugs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slug == null)
            {
                return NotFound();
            }

            return View(slug);
        }

        // GET: Admin/Slugs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Slugs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slug slug)
        {
            if (ModelState.IsValid)
            {
                _context.Add(slug);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slug);
        }

        // GET: Admin/Slugs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slug = await _context.Slugs.FindAsync(id);
            if (slug == null)
            {
                return NotFound();
            }
            return View(slug);
        }

        // POST: Admin/Slugs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Slug slug)
        {
            if (id != slug.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(slug);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SlugExists(slug.Id))
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
            return View(slug);
        }

        // GET: Admin/Slugs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slug = await _context.Slugs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slug == null)
            {
                return NotFound();
            }

            return View(slug);
        }

        // POST: Admin/Slugs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slug = await _context.Slugs.FindAsync(id);
            _context.Slugs.Remove(slug);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SlugExists(int id)
        {
            return _context.Slugs.Any(e => e.Id == id);
        }

        public async Task<IActionResult> GenerateSlugs()
        {
            await SlugService.GenerateSlugs();
            return RedirectToAction("Index", "Audiobooks", new { area = "Admin"} );

        }
    }
}
