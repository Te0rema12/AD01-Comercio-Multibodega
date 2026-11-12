
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly PedidosDbContext _context;

        public ClientesController(PedidosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Clientes.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Cliente datos)
        {
            if (string.IsNullOrWhiteSpace(datos.Nombres) ||
                string.IsNullOrWhiteSpace(datos.Cedula))
                return BadRequest(new
                {
                    mensaje = "Nombre y cédula son obligatorios."
                });

            if (datos.Cedula.Length != 10 ||
                !datos.Cedula.All(char.IsDigit))
                return BadRequest(new
                {
                    mensaje = "La cédula debe contener 10 dígitos."
                });

            if (await _context.Clientes.AnyAsync(
                c => c.Cedula == datos.Cedula))
                return BadRequest(new
                {
                    mensaje = "La cédula ya está registrada."
                });

            var cliente = new Cliente
            {
                Nombres = datos.Nombres.Trim(),
                Cedula = datos.Cedula,
                Correo = datos.Correo
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = cliente.IdCliente },
                cliente
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Cliente datos)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            if (string.IsNullOrWhiteSpace(datos.Nombres) ||
                string.IsNullOrWhiteSpace(datos.Cedula) ||
                datos.Cedula.Length != 10 ||
                !datos.Cedula.All(char.IsDigit))
                return BadRequest(new
                {
                    mensaje = "Nombre o cédula inválidos."
                });

            if (await _context.Clientes.AnyAsync(
                c => c.Cedula == datos.Cedula &&
                     c.IdCliente != id))
                return BadRequest(new
                {
                    mensaje = "La cédula pertenece a otro cliente."
                });

            cliente.Nombres = datos.Nombres.Trim();
            cliente.Cedula = datos.Cedula;
            cliente.Correo = datos.Correo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Cliente actualizado correctamente."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            if (await _context.Pedidos.AnyAsync(
                p => p.IdCliente == id))
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar un cliente con pedidos."
                });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Cliente eliminado correctamente."
            });
        }
    }
}
