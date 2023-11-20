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
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ErrorReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IAudiobookService AudiobookService { get; }

        public ErrorReportController(ApplicationDbContext context,
            IAudiobookService audiobookService)
        {
            _context = context;
            AudiobookService = audiobookService;
        }

        // GET: Admin/Blurbs
        public async Task<IActionResult> Index()
        {
            return View(await AudiobookService.GetAllErrorReports());
        }

        // GET: Admin/Blurbs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blurb = await AudiobookService.GetErrorReportById(id.Value);
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
        public async Task<IActionResult> Create(ErrorReport obj)
        {
            if (ModelState.IsValid)
            {
                await AudiobookService.AddErrorReport(obj);
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }

        // GET: Admin/Blurbs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var blurb = await AudiobookService.GetErrorReportById(id.Value);
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
        public async Task<IActionResult> Edit(int id, ErrorReport obj)
        {
            if (id != obj.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AudiobookService.EditErrorReport(id, obj);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ErrorReportExists(obj.Id))
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
            return View(obj);
        }

        // GET: Admin/Blurbs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var blurb = await AudiobookService.GetErrorReportById(id.Value);
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
            await AudiobookService.DeleteErrorReport(id);
            return RedirectToAction(nameof(Index));
        }

        private bool ErrorReportExists(int id)
        {
            return _context.ErrorReports.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ToggleBookHold(int id)
        {
            var errorReport = await AudiobookService.GetErrorReportById(id);
            if (errorReport != null)
            {
                var audibook = await AudiobookService.GetAudiobookById(errorReport.AudiobookId.Value);

                audibook.Error = !audibook.Error;
                await AudiobookService.EditAudiobook(audibook.Id, audibook);
                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }

        public async Task<IActionResult> ResolveErrorReport(int id)
        {
            var errorReport = await AudiobookService.GetErrorReportById(id);
            if (errorReport != null)
            {
                errorReport.ErrorStatus = ErrorStatus.Resolved;

                var audiobook = await AudiobookService.GetAudiobookById(errorReport.AudiobookId.Value);
                if (audiobook.Error)
                {
                    audiobook.Error = !audiobook.Error;
                    await AudiobookService.EditAudiobook(audiobook.Id, audiobook);
                }
                await AudiobookService.EditErrorReport(errorReport.Id, errorReport);

                _context.Entry(audiobook).State = EntityState.Detached;
                _context.Entry(errorReport).State = EntityState.Detached;

                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }

        

        public async Task<IActionResult> ReportError(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var audiobook = await AudiobookService.GetAudiobookById(id);
            if (audiobook == null)
            {
                return NotFound();
            }
            var errorReport = new ErrorReport();
            errorReport.ErrorStatus = ErrorStatus.New;
            errorReport.AudiobookId = audiobook.Id;

            await AudiobookService.AddErrorReport(errorReport);

            audiobook.Error = true;
            await AudiobookService.EditAudiobook(audiobook.Id, audiobook);

            return RedirectToAction(nameof(Index), "Audiobooks", new { area = "Admin" });
        }
    }
}
