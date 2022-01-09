using Domain.Models;
using System.Collections.Generic;

namespace Memento.Web.Models
{
    public class ReasonsModel
    {
        public ReasonsModel() {
            Reasons = new List<ReasonModel>();
        }
        public List<ReasonModel> Reasons { get; set; }

    }
}