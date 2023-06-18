using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections;
using System.Collections.Generic;

namespace Audiobooks.Dtos
{
    public class GetSeriesDto
    {
        public GetSeriesDto()
        {
            Books = new List<GetAudiobookDto>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GetAudiobookDto> Books { get; set; }
    }
}
