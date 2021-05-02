using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class AlbumExport
    {
        public int AlbumExportId { get; set; }
        [MaxLength(100)]
        public string AlbumPublicId { get; set; }
        public int AlbumId { get; set; }

        [ForeignKey("AlbumId")]
        [InverseProperty("AlbumExports")]
        public virtual Album Album { get; set; }
    }
}
