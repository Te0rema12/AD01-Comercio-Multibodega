
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models
{
    [Table("Existencia")]
    public class Existencia
    {
        [Key]
        public int IdExistencia { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int IdBodega { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [ForeignKey(nameof(IdBodega))]
        public Bodega? Bodega { get; set; }
    }
}
