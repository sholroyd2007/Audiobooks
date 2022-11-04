using System.Xml.Serialization;

namespace Audiobooks.Services
{
	[XmlRoot(ElementName = "url", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
	public class Url
	{

		[XmlElement(ElementName = "loc", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
		public string Loc { get; set; }

		[XmlElement(ElementName = "lastmod", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
		public string Lastmod { get; set; }
	}
}