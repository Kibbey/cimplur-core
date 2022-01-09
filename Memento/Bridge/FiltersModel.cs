using System.Collections.Generic;

namespace Bridge
{
    public class FiltersModel
    {
        public List<PersonSelectedModel> AllPeople { get; set; }
        public List<AlbumViewModel> Albums { get; set; }
        public List<int> Years { get; set; }
        public bool? Me { get; set; }
    }
}
