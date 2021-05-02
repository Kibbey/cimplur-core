using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Album
    {
        public Album() {
            AlbumDrops = new List<AlbumDrop>();
            AlbumExports = new List<AlbumExport>();
        }
        public int AlbumId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public bool Archived { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }
        
        [ForeignKey("UserId")]
        [InverseProperty("Albums")]
        public virtual UserProfile Owner { get; set; }
        [InverseProperty("Album")]
        public List<AlbumDrop> AlbumDrops { get; set; }
        [InverseProperty("Album")]
        public List<AlbumExport> AlbumExports { get; set; }
    }
}
