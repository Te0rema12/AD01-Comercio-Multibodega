
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models
{
    [Table("MovimientoInventario")]
    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int IdBodega { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        public DateTime? Fecha { get; set; }

        [ForeignKey(nameof(IdBodega))]
        public Bodega? Bodega { get; set; }
    }
}
