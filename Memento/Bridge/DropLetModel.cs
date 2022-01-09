using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge
{
    public class DropletModel
    {
        public int DropletId { get; set;}
        public int ContentId { get; set; }
        public int DropId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }

        public virtual ContentModel Content { get; set; }
        public virtual DropModel Drop { get; set; }        
    }
}
