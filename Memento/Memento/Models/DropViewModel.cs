using System.Collections.Generic;


namespace Memento.Web.Models
{
    public class DropViewModel : DropModel
    {
       
        public Dictionary<int,string> Tags { get; set; }
        public List<int> SelectedTags { get; set; }
    }
}