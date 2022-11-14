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
using Microsoft.AspNetCore.Authorization;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class BlurbsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IAudiobookService AudiobookService { get; }

        public BlurbsController(ApplicationDbContext context,
            IAudiobookService audiobookService)
        {
            _context = context;
            AudiobookService = audiobookService;
        }

        // GET: Admin/Blurbs
        public async Task<IActionResult> Index()
        {
            return View(await AudiobookService.GetBlurbs());
        }

        // GET: Admin/Blurbs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blurb = await AudiobookService.GetBlurbById(id.Value);
            if (blurb != null)
            {
                return View(blurb);
            }
            return NotFound();
        }

        // GET: Admin/Blurbs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Blurbs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blurb blurb)
        {
            if (ModelState.IsValid)
            {
                await AudiobookService.AddBlurb(blurb);
                return RedirectToAction(nameof(Index));
            }
            return View(blurb);
        }

        // GET: Admin/Blurbs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var blurb = await AudiobookService.GetBlurbById(id.Value);
                if (blurb != null)
                {
                    return View(blurb);
                }
                return NotFound();
            }
            return NotFound();
        }

        // POST: Admin/Blurbs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blurb blurb)
        {
            if (id != blurb.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AudiobookService.EditBlurb(blurb);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlurbExists(blurb.Id))
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
            return View(blurb);
        }

        // GET: Admin/Blurbs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var blurb = await AudiobookService.GetBlurbById(id.Value);
                if (blurb != null)
                {
                    return View(blurb);
                }
                return NotFound();
            }
            return NotFound();
        }

        // POST: Admin/Blurbs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await AudiobookService.DeleteBlurb(id);
            return RedirectToAction(nameof(Index));
        }

        private bool BlurbExists(int id)
        {
            return _context.Blurb.Any(e => e.Id == id);
        }
    }
}
