
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models
{
    [Table("ReservaStock")]
    public class ReservaStock
    {
        [Key]
        public int IdReserva { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int IdBodega { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        public bool Estado { get; set; }

        [ForeignKey(nameof(IdBodega))]
        public Bodega? Bodega { get; set; }
    }
}
