
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly PedidosDbContext _context;

        public PedidosController(PedidosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .ToListAsync();

            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound(new { mensaje = "Pedido no encontrado." });

            return Ok(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Pedido datos)
        {
            if (!await _context.Clientes.AnyAsync(
                c => c.IdCliente == datos.IdCliente))
                return BadRequest(new
                {
                    mensaje = "El cliente indicado no existe."
                });

            var pedido = new Pedido
            {
                IdCliente = datos.IdCliente,
                Fecha = DateTime.Now,
                Total = 0,
                Estado = true
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = pedido.IdPedido },
                pedido
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Pedido datos)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
                return NotFound(new { mensaje = "Pedido no encontrado." });

            if (!await _context.Clientes.AnyAsync(
                c => c.IdCliente == datos.IdCliente))
                return BadRequest(new
                {
                    mensaje = "El cliente indicado no existe."
                });

            pedido.IdCliente = datos.IdCliente;
            pedido.Estado = datos.Estado;


            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Pedido actualizado correctamente."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
                return NotFound(new { mensaje = "Pedido no encontrado." });

            if (await _context.DetallesPedido.AnyAsync(
                d => d.IdPedido == id))
                return BadRequest(new
                {
                    mensaje = "El pedido tiene detalles registrados."
                });

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Pedido eliminado correctamente."
            });
        }
    }
}
