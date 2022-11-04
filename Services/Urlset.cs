using System.Collections.Generic;
using System.Xml.Serialization;

namespace Audiobooks.Services
{

    public partial class SiteMapService
    {
        [XmlRoot(ElementName = "urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
        public class Urlset
        {

            [XmlElement(ElementName = "url", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
            public List<Url> Url { get; set; }

            [XmlAttribute(AttributeName = "xmlns", Namespace = "")]
            public string Xmlns { get; set; }

            [XmlAttribute(AttributeName = "video", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Video { get; set; }

            [XmlAttribute(AttributeName = "image", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Image { get; set; }

            [XmlAttribute(AttributeName = "mobile", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Mobile { get; set; }

            [XmlText]
            public string Text { get; set; }
        }
    }
}
