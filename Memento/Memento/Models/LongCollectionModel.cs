using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class LongCollectionModel
    {
        public LongCollectionModel() {
            Ids = new List<long>();
        }

        public List<long> Ids { get; set; }
    }
}