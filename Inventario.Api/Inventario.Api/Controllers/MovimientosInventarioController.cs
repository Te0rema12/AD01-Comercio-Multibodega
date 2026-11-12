
using System.Data;
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosInventarioController : ControllerBase
    {
        private readonly InventarioDbContext _context;

        public MovimientosInventarioController(InventarioDbContext context)
        {
            _context = context;
        }

    
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.MovimientosInventario
                .AsNoTracking()
                .Include(m => m.Bodega)
                .OrderByDescending(m => m.IdMovimiento)
                .ToListAsync());
        }

   
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var movimiento = await _context.MovimientosInventario
                .AsNoTracking()
                .Include(m => m.Bodega)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento == null)
                return NotFound(new { mensaje = "Movimiento no encontrado." });

            return Ok(movimiento);
        }

   
        [HttpPost]
        public async Task<IActionResult> Post(MovimientoInventario datos)
        {
            if (datos.IdProducto <= 0 || datos.IdBodega <= 0 ||
                datos.Cantidad <= 0)
                return BadRequest(new
                {
                    mensaje = "Producto, bodega y cantidad deben ser mayores que cero."
                });

            string tipo = datos.Tipo?.Trim().ToUpperInvariant() ?? "";

            if (tipo != "ENTRADA" && tipo != "SALIDA")
                return BadRequest(new
                {
                    mensaje = "El tipo debe ser ENTRADA o SALIDA."
                });

            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var existencia = await _context.Existencias
                .FirstOrDefaultAsync(e =>
                    e.IdProducto == datos.IdProducto &&
                    e.IdBodega == datos.IdBodega);

            if (existencia == null)
                return BadRequest(new
                {
                    mensaje = "Primero debe registrar la existencia del producto en la bodega."
                });

            if (tipo == "ENTRADA")
            {
                if (existencia.Stock > int.MaxValue - datos.Cantidad)
                    return BadRequest(new
                    {
                        mensaje = "La cantidad supera el límite permitido."
                    });

                existencia.Stock += datos.Cantidad;
            }
            else
            {
                int reservado = await _context.ReservasStock
                    .Where(r => r.IdProducto == datos.IdProducto &&
                                r.IdBodega == datos.IdBodega &&
                                r.Estado)
                    .SumAsync(r => (int?)r.Cantidad) ?? 0;

                long disponible = (long)existencia.Stock - reservado;

                if (datos.Cantidad > disponible)
                    return BadRequest(new
                    {
                        mensaje = "Stock disponible insuficiente.",
                        stockFisico = existencia.Stock,
                        stockReservado = reservado,
                        stockDisponible = disponible
                    });

                existencia.Stock -= datos.Cantidad;
            }

            var movimiento = new MovimientoInventario
            {
                IdProducto = datos.IdProducto,
                IdBodega = datos.IdBodega,
                Tipo = tipo,
                Cantidad = datos.Cantidad,
                Fecha = DateTime.Now
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return CreatedAtAction(nameof(Get),
                new { id = movimiento.IdMovimiento }, movimiento);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id)
        {
            bool existe = await _context.MovimientosInventario
                .AnyAsync(m => m.IdMovimiento == id);

            if (!existe)
                return NotFound(new { mensaje = "Movimiento no encontrado." });

            return BadRequest(new
            {
                mensaje = "Los movimientos registrados no se editan. Realice un movimiento compensatorio."
            });
        }

   
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool existe = await _context.MovimientosInventario
                .AnyAsync(m => m.IdMovimiento == id);

            if (!existe)
                return NotFound(new { mensaje = "Movimiento no encontrado." });

            return BadRequest(new
            {
                mensaje = "Los movimientos no se eliminan porque forman parte del historial."
            });
        }
    }
}
