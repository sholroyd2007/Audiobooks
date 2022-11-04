using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static Audiobooks.Services.SiteMapService;

namespace Audiobooks
{
    public static class XmlHelper
    {
        public static string ToSitemapXmlString(Urlset value)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true,
                CheckCharacters = false,
                Encoding = Encoding.UTF8
            };

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "http://www.sitemaps.org/schemas/sitemap/0.9");

            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream, settings);
            var serializer = new XmlSerializer(value.GetType());
            serializer.Serialize(writer, value, ns);
            return stream.ToString();
        }
    }
}
