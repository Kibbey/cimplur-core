using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class StreamRequestModel
    {
        public StreamRequestModel() {
            AlbumIds = new List<int>();
            PeopleIds = new List<int>();
        }
        public List<int> AlbumIds { get; set; }
        public List<int> PeopleIds { get; set; }
        public int Skip { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public bool? IncludeMe { get; set; }
        public bool? Chronological { get; set; }
        public int Year { get; set; }
    }
}