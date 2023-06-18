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
    public class NarratorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public INarratorService NarratorService { get; }

        public NarratorsController(ApplicationDbContext context,
            INarratorService narratorService)
        {
            _context = context;
            NarratorService = narratorService;
        }

        // GET: Admin/Narrators
        public async Task<IActionResult> Index()
        {
              return View(await NarratorService.GetNarrators());
        }

        // GET: Admin/Narrators/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Narrators == null)
            {
                return NotFound();
            }

            var narrator = await NarratorService.GetNarratorById(id.Value);
            if (narrator == null)
            {
                return NotFound();
            }

            return View(narrator);
        }

        // GET: Admin/Narrators/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Narrators/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Narrator narrator)
        {
            if (ModelState.IsValid)
            {
                await NarratorService.AddNarrator(narrator);
                return RedirectToAction(nameof(Index));
            }
            return View(narrator);
        }

        // GET: Admin/Narrators/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Narrators == null)
            {
                return NotFound();
            }

            var narrator = await NarratorService.GetNarratorById(id.Value);
            if (narrator == null)
            {
                return NotFound();
            }
            return View(narrator);
        }

        // POST: Admin/Narrators/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Narrator narrator)
        {
            if (id != narrator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await NarratorService.EditNarrator(narrator);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NarratorExists(narrator.Id))
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
            return View(narrator);
        }

        // GET: Admin/Narrators/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Narrators == null)
            {
                return NotFound();
            }

            var narrator = await NarratorService.GetNarratorById(id.Value);
            if (narrator == null)
            {
                return NotFound();
            }

            return View(narrator);
        }

        // POST: Admin/Narrators/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Narrators == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Narrators'  is null.");
            }
            await NarratorService.DeleteNarrator(id);
            return RedirectToAction(nameof(Index));
        }

        private bool NarratorExists(int id)
        {
          return _context.Narrators.Any(e => e.Id == id);
        }
    }
}
