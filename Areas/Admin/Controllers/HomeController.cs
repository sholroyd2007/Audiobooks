using Audiobooks.Data;
using Audiobooks.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class HomeController : Controller
    {
        public HomeController(ApplicationDbContext databaseContext,
            IAudiobookService audiobookService)
        {
            DatabaseContext = databaseContext;
            AudiobookService = audiobookService;
        }

        public ApplicationDbContext DatabaseContext { get; }
        public IAudiobookService AudiobookService { get; }

        public async Task<IActionResult> DownloadCatalogue()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Audiobooks");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Name";
                worksheet.Cell(currentRow, 2).Value = "CategoryId";
                worksheet.Cell(currentRow, 3).Value = "Author";
                worksheet.Cell(currentRow, 4).Value = "Narrator";
                worksheet.Cell(currentRow, 5).Value = "Url";
                worksheet.Cell(currentRow, 6).Value = "ImageUrl";
                worksheet.Cell(currentRow, 7).Value = "Length";
                worksheet.Cell(currentRow, 8).Value = "DateAdded";
                worksheet.Cell(currentRow, 9).Value = "Description";
                worksheet.Cell(currentRow, 10).Value = "Series";
                worksheet.Cell(currentRow, 11).Value = "SeriesNumber";
                foreach (var item in await AudiobookService.GetAllAudiobooks())
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item?.Name;
                    worksheet.Cell(currentRow, 2).Value = item?.Category?.Id;
                    worksheet.Cell(currentRow, 3).Value = item?.Author;
                    worksheet.Cell(currentRow, 4).Value = item?.Narrator;
                    worksheet.Cell(currentRow, 5).Value = item?.Url;
                    worksheet.Cell(currentRow, 6).Value = item?.ImageUrl;
                    worksheet.Cell(currentRow, 7).Value = item?.Length;
                    worksheet.Cell(currentRow, 8).Value = item?.DateAdded;
                    worksheet.Cell(currentRow, 9).Value = item?.Description;
                    worksheet.Cell(currentRow, 10).Value = item?.Series;
                    worksheet.Cell(currentRow, 11).Value = item?.SeriesNumber;
                }

               
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Audiobooks_List_Full.xlsx");
                }
            }
        }
    }
}

