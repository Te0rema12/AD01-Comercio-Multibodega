
using System.Data;
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasStockController : ControllerBase
    {
        private readonly InventarioDbContext _context;

        public ReservasStockController(InventarioDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.ReservasStock
                .AsNoTracking()
                .Include(r => r.Bodega)
                .ToListAsync());
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var reserva = await _context.ReservasStock
                .AsNoTracking()
                .Include(r => r.Bodega)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
                return NotFound(new { mensaje = "Reserva no encontrada." });

            return Ok(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ReservaStock datos)
        {
            if (datos.IdProducto <= 0 || datos.IdBodega <= 0 ||
                datos.Cantidad <= 0)
                return BadRequest(new
                {
                    mensaje = "Producto, bodega y cantidad deben ser mayores que cero."
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
                    mensaje = "No existe inventario para este producto y bodega."
                });

            int reservado = await _context.ReservasStock
                .Where(r => r.IdProducto == datos.IdProducto &&
                            r.IdBodega == datos.IdBodega &&
                            r.Estado)
                .SumAsync(r => (int?)r.Cantidad) ?? 0;

            long disponible = (long)existencia.Stock - reservado;

            if (datos.Cantidad > disponible)
                return BadRequest(new
                {
                    mensaje = "No existe suficiente stock disponible.",
                    stockDisponible = disponible
                });

            var reserva = new ReservaStock
            {
                IdProducto = datos.IdProducto,
                IdBodega = datos.IdBodega,
                Cantidad = datos.Cantidad,
                Estado = true
            };

            _context.ReservasStock.Add(reserva);
            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return CreatedAtAction(nameof(Get),
                new { id = reserva.IdReserva }, reserva);
        }

      
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, ReservaStock datos)
        {
            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var reserva = await _context.ReservasStock.FindAsync(id);

            if (reserva == null)
                return NotFound(new { mensaje = "Reserva no encontrada." });

            if (datos.IdProducto != reserva.IdProducto ||
                datos.IdBodega != reserva.IdBodega ||
                datos.Cantidad != reserva.Cantidad)
            {
                return BadRequest(new
                {
                    mensaje = "No se permite cambiar el producto, bodega o cantidad de una reserva."
                });
            }

            if (!reserva.Estado && datos.Estado)
                return BadRequest(new
                {
                    mensaje = "Una reserva cancelada no puede reactivarse. Cree una nueva."
                });

            reserva.Estado = datos.Estado;

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return Ok(new
            {
                mensaje = reserva.Estado
                    ? "La reserva permanece activa."
                    : "Reserva cancelada correctamente."
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reserva = await _context.ReservasStock.FindAsync(id);

            if (reserva == null)
                return NotFound(new { mensaje = "Reserva no encontrada." });

            return BadRequest(new
            {
                mensaje = "No se eliminan reservas físicamente. Utilice PUT con Estado=false para cancelarla."
            });
        }
    }
}
