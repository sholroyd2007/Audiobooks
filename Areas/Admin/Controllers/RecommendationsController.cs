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
    public class RecommendationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IAudiobookService AudiobookService { get; }

        public RecommendationsController(ApplicationDbContext context,
            IAudiobookService audiobookService)
        {
            _context = context;
            AudiobookService = audiobookService;
        }

        // GET: Admin/Recommendations
        public async Task<IActionResult> Index()
        {
            return View(await AudiobookService.GetRecommendations());
        }

        // GET: Admin/Recommendations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                var recommendation = await AudiobookService.GetRecommendationById(id.Value);
                if (recommendation != null)
                {
                    return View(recommendation);
                }
                return NotFound();
            }
            return NotFound();
        }

        // GET: Admin/Recommendations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Recommendations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Recommendation recommendation)
        {
            if (ModelState.IsValid)
            {
                await AudiobookService.AddRecommendation(recommendation);
                return RedirectToAction(nameof(Index));
            }
            return View(recommendation);
        }

        // GET: Admin/Recommendations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var recommendation = await AudiobookService.GetRecommendationById(id.Value);
                if (recommendation != null)
                {
                    return View(recommendation);
                }
                return NotFound();
            }
            return NotFound();


        }

        // POST: Admin/Recommendations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Recommendation recommendation)
        {
            if (id != recommendation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AudiobookService.EditRecommendation(recommendation);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecommendationExists(recommendation.Id))
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
            return View(recommendation);
        }

        // GET: Admin/Recommendations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var recommendation = await AudiobookService.GetRecommendationById(id.Value);
                if (recommendation != null)
                {
                    return View(recommendation);
                }
                return NotFound();
            }
            return NotFound();
        }

        // POST: Admin/Recommendations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await AudiobookService.DeleteRecommendation(id);
            return RedirectToAction(nameof(Index));
        }

        private bool RecommendationExists(int id)
        {
            return _context.Recommendation.Any(e => e.Id == id);
        }
    }
}
