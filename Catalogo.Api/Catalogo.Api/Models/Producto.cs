
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalogo.Api.Models
{
    [Table("Producto")]
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        public int IdCategoria { get; set; }

        public int IdMarca { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public bool Estado { get; set; }

        [ForeignKey(nameof(IdCategoria))]
        public Categoria? Categoria { get; set; }

        [ForeignKey(nameof(IdMarca))]
        public Marca? Marca { get; set; }
    }
}
