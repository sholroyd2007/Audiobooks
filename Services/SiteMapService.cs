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
            IAudiobookService audiobookService,
            ISlugService slugService)
        {
            Context = context;
            AudiobookService = audiobookService;
            SlugService = slugService;
        }

        public ApplicationDbContext Context { get; }
        public IAudiobookService AudiobookService { get; }
        public ISlugService SlugService { get; }

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
                var slug = await SlugService.GetSlugForEntity(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Detail/{slug.Name}", Lastmod = DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd") });
            }

            foreach (var item in categories)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookCategory(item.Id);
                var slug = await SlugService.GetSlugForEntity(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Category/{slug.Name}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            foreach (var item in authors)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookAuthor(item.Id);
                var slug = await SlugService.GetSlugForEntity(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Author/{slug.Name}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            foreach (var item in narrators)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookNarrator(item.Id);
                var slug = await SlugService.GetSlugForEntity(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/Narrator/{slug.Name}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            foreach (var item in series)
            {
                var lastMod = await AudiobookService.GetXmlLastModBookSeries(item.Id);
                var slug = await SlugService.GetSlugForEntity(item);
                urlSet.Url.Add(new Url { Loc = $"http://audio-bux.link/Home/BookSeries/{slug.Name}", Lastmod = lastMod.ToString("yyyy-MM-dd") });
            }

            var xml = XmlHelper.ToSitemapXmlString(urlSet);
            return xml;
        }
    }
}
