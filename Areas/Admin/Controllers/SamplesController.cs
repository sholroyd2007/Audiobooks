using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Audiobooks.Data;
using Audiobooks.Models;
using Microsoft.AspNetCore.Authorization;
using Audiobooks.Services;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class SamplesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IAudiobookService AudiobookService { get; }

        public SamplesController(ApplicationDbContext context,
            IAudiobookService audiobookService)
        {
            _context = context;
            AudiobookService = audiobookService;
        }

        // GET: Admin/Samples
        public async Task<IActionResult> Index()
        {
            return View(await AudiobookService.GetSamples());
        }

        // GET: Admin/Samples/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                var sample = await AudiobookService.GetSampleById(id.Value);
                if (sample != null)
                {
                    return View(sample);
                }
                return NotFound();
            }
            return NotFound();

        }

        // GET: Admin/Samples/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Samples/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sample sample)
        {
            if (ModelState.IsValid)
            {
                var file = HttpContext.Request.Form.Files.FirstOrDefault();
                if (file != null)
                {
                    sample.Data = file.OpenReadStream().ReadFully();
                    sample.ContentType = file.ContentType;
                }

                await AudiobookService.AddSample(sample);
                return RedirectToAction(nameof(Index));
            }
            return View(sample);
        }

        // GET: Admin/Samples/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var sample = await _context.Sample.FindAsync(id);
                if (sample != null)
                {
                    return View(sample);
                }
                return NotFound();
            }
            return NotFound();
        }

        // POST: Admin/Samples/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sample sample)
        {
            if (id != sample.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AudiobookService.EditSample(sample);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SampleExists(sample.Id))
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
            return View(sample);
        }

        // GET: Admin/Samples/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var sample = await AudiobookService.GetSampleById(id.Value);
                if (sample != null)
                {
                    return View(sample);
                }
                return NotFound();
            }
            return NotFound();
        }

        // POST: Admin/Samples/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await AudiobookService.DeleteSample(id);
            return RedirectToAction(nameof(Index));
        }

        private bool SampleExists(int id)
        {
            return _context.Sample.Any(e => e.Id == id);
        }
    }
}
