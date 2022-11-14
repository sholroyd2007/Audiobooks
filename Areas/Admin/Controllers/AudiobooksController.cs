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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AudiobooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IAudiobookService AudiobookService { get; }

        public AudiobooksController(ApplicationDbContext context,
            IAudiobookService audiobookService)
        {
            _context = context;
            AudiobookService = audiobookService;
        }

        // GET: Audiobooks

        public async Task<IActionResult> Index()
        {
            var audiobooks = await AudiobookService.GetAllAudiobooks();
            return View(audiobooks);
        }

        // GET: Audiobooks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                var audiobook = await AudiobookService.GetAudiobookById(id.Value);
                if (audiobook != null)
                {
                    return View(audiobook);
                }
                return NotFound();
            }
            return NotFound();
            
        }

        // GET: Audiobooks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Audiobooks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Audiobook audiobook)
        {
            if (ModelState.IsValid)
            {
                await AudiobookService.AddAudiobook(audiobook);                
                return RedirectToAction(nameof(Index));
            }
            return View(audiobook);
        }

        // GET: Audiobooks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var audiobook = await AudiobookService.GetAudiobookById(id.Value);
            if (audiobook != null)
            {
                return View(audiobook);
            }
            return NotFound();
            
        }

        [HttpPost]
        public async Task<IActionResult> AddSample(int id)
        {
            var audiobook = await AudiobookService.GetAudiobookById(id);
            var sample = await AudiobookService.GetSampleByAudiobookId(id);
            var file = HttpContext.Request.Form.Files.FirstOrDefault();
            if (file != null)
            {
                var newSample = new Sample
                {
                    AudiobookId = audiobook.Id,
                    Data = file.OpenReadStream().ReadFully(),
                    ContentType = file.ContentType
                };
                if (sample != null)
                {
                    _context.Sample.Remove(sample);
                    await _context.SaveChangesAsync();
                }
                _context.Sample.Add(newSample);
                await _context.SaveChangesAsync();
            };
            return RedirectToAction(nameof(Edit), new { id = id });

        }

        // POST: Audiobooks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Audiobook audiobook)
        {
            if (id != audiobook.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AudiobookService.EditAudiobook(id, audiobook);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AudiobookExists(audiobook.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return View(audiobook);
            }
            return View(audiobook);
        }

        // GET: Audiobooks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var audiobook = await AudiobookService.GetAudiobookById(id.Value);
            if (audiobook != null)
            {
                return View(audiobook);
                
            }
            return NotFound();

        }

        // POST: Audiobooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await AudiobookService.DeleteAudiobook(id);            
            return RedirectToAction(nameof(Index));
        }

        private bool AudiobookExists(int id)
        {
            return _context.Audiobook.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ImportCatalogue(IFormFile file, [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            if (file != null)
            {
                await AudiobookService.ImportCatalogue(file, hostingEnvironment);
            }
            return RedirectToAction(nameof(Index), new { Controller = "Audiobooks", Area = "Admin" });
        }

        public async Task<IActionResult> DeleteAllBooks(int id)
        {
            await AudiobookService.DeleteAllAudiobooks();
            return RedirectToAction(nameof(Index));
        }
    }
}
