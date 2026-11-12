
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetallesPedidoController : ControllerBase
    {
        private readonly PedidosDbContext _context;

        public DetallesPedidoController(PedidosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.DetallesPedido.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var detalle = await _context.DetallesPedido.FindAsync(id);

            if (detalle == null)
                return NotFound(new
                {
                    mensaje = "Detalle no encontrado."
                });

            return Ok(detalle);
        }

        private async Task RecalcularTotal(int idPedido)
        {
            var pedido = await _context.Pedidos.FindAsync(idPedido);

            if (pedido == null)
                return;

            pedido.Total = await _context.DetallesPedido
                .Where(d => d.IdPedido == idPedido)
                .SumAsync(d => (decimal?)(d.Cantidad * d.PrecioUnitario))
                ?? 0;

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Post(DetallePedido datos)
        {
            if (!await _context.Pedidos.AnyAsync(
                p => p.IdPedido == datos.IdPedido))
                return BadRequest(new
                {
                    mensaje = "El pedido indicado no existe."
                });

            if (datos.IdProducto <= 0 ||
                datos.Cantidad <= 0 ||
                datos.PrecioUnitario <= 0)
                return BadRequest(new
                {
                    mensaje = "Producto, cantidad o precio inválidos."
                });

            var detalle = new DetallePedido
            {
                IdPedido = datos.IdPedido,
                IdProducto = datos.IdProducto,
                Cantidad = datos.Cantidad,
                PrecioUnitario = datos.PrecioUnitario
            };

            _context.DetallesPedido.Add(detalle);
            await _context.SaveChangesAsync();

            await RecalcularTotal(detalle.IdPedido);

            return CreatedAtAction(
                nameof(Get),
                new { id = detalle.IdDetalle },
                detalle
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            int id, DetallePedido datos)
        {
            var detalle = await _context.DetallesPedido.FindAsync(id);

            if (detalle == null)
                return NotFound(new
                {
                    mensaje = "Detalle no encontrado."
                });

            if (!await _context.Pedidos.AnyAsync(
                p => p.IdPedido == datos.IdPedido))
                return BadRequest(new
                {
                    mensaje = "El pedido indicado no existe."
                });

            if (datos.IdProducto <= 0 ||
                datos.Cantidad <= 0 ||
                datos.PrecioUnitario <= 0)
                return BadRequest(new
                {
                    mensaje = "Datos inválidos."
                });

            int pedidoAnterior = detalle.IdPedido;

            detalle.IdPedido = datos.IdPedido;
            detalle.IdProducto = datos.IdProducto;
            detalle.Cantidad = datos.Cantidad;
            detalle.PrecioUnitario = datos.PrecioUnitario;

            await _context.SaveChangesAsync();

            await RecalcularTotal(pedidoAnterior);

            if (pedidoAnterior != detalle.IdPedido)
                await RecalcularTotal(detalle.IdPedido);

            return Ok(new
            {
                mensaje = "Detalle actualizado correctamente."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var detalle = await _context.DetallesPedido.FindAsync(id);

            if (detalle == null)
                return NotFound(new
                {
                    mensaje = "Detalle no encontrado."
                });

            int idPedido = detalle.IdPedido;

            _context.DetallesPedido.Remove(detalle);
            await _context.SaveChangesAsync();

            await RecalcularTotal(idPedido);

            return Ok(new
            {
                mensaje = "Detalle eliminado correctamente."
            });
        }
    }
}
