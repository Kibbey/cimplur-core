using System;

namespace Domain.Entities
{
    public class AlbumDrop
    {
        // moved to fluent
        //[Column(Order = 0), Key]
        public int AlbumId { get; set; }
        //[Column(Order = 1), Key]
        public int DropId { get; set; }
        public DateTime Created { get; set; }

        public virtual Album Album { get; set; }
        public virtual Drop Drop { get; set; }
    }
}
