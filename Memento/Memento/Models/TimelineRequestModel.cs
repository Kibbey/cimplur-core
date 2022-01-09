using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class TimelineRequestModel
    {
        public TimelineRequestModel() {
        }

        public int Skip { get; set; }
        public bool Ascending { get; set; }
    }
}