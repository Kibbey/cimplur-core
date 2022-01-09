using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge
{
    public class DropViewModel
    {
        public DropViewModel() {
            Drops = new List<DropModel>();
        }
        public List<DropModel> Drops { get; set;  }
        public int Skip { get; set; }
        public bool Done { get; set; }
    }
}
