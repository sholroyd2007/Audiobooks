using Audiobooks.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Services
{
    public interface ISiteMapService
    {
        Task<string> GetSitemapXml();
    }

    public partial class SiteMapService : ISiteMapService
    {
        public SiteMapService(ApplicationDbContext context,
            IAudiobookService audiobookService)
        {
            Context = context;
            AudiobookService = audiobookService;
        }

        public ApplicationDbContext Context { get; }
        public IAudiobookService AudiobookService { get; }

        public async Task<string> GetSitemapXml()
        {
            var books = await AudiobookService.GetAllAudiobooks();
            var categories = await AudiobookService.GetCategories();
            var authors = await AudiobookService.GetAuthors();
            var narrators = await AudiobookService.GetNarrators();
            var series = await AudiobookService.GetBookSeries();
            
            var urlSet = new Urlset();
            urlSet.Url = new System.Collections.Generic.List<Url>();

            var urls = new string[] { "", "Recommendations", "Browse", "Search" };
            foreach (var item in urls)
            {
                urlSet.Url.Add(new Url { Loc = $"https://audio-bux.link/{(!string.IsNullOrWhiteSpace(item) ? "Home/" : "")}{item}", Lastmod = DateTime.UtcNow.AddDays(-14).ToString("yyyy-MM-dd") });
            }

            foreach (var item in books)
            {
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Detail/{item.Id}", Lastmod = item.DateAdded.ToString("yyyy-MM-dd")});
            }

            foreach (var item in categories)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookCategory(item.Id);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Category/{item.Id}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            foreach (var item in authors)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookAuthor(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Author?author={item}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            foreach (var item in narrators)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookNarrator(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Narrator?narrator={item}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            foreach (var item in series)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookSeries(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/BookSeries?name={item}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            var xml = XmlHelper.ToSitemapXmlString(urlSet);
            return xml;
        }
    }
}
