using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class SelectedIdModel
    {
        public SelectedIdModel() {
            Ids = new List<int>();
        }

        public List<int> Ids { get; set; }
    }
}