
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExistenciasController : ControllerBase
    {
        private readonly InventarioDbContext _context;

        public ExistenciasController(InventarioDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var datos = await _context.Existencias
                .AsNoTracking()
                .Include(e => e.Bodega)
                .ToListAsync();

            return Ok(datos);
        }

     
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var existencia = await _context.Existencias
                .AsNoTracking()
                .Include(e => e.Bodega)
                .FirstOrDefaultAsync(e => e.IdExistencia == id);

            if (existencia == null)
                return NotFound(new { mensaje = "Existencia no encontrada." });

            return Ok(existencia);
        }

    
        [HttpPost]
        public async Task<IActionResult> Post(Existencia datos)
        {
            if (datos.IdProducto <= 0 || datos.IdBodega <= 0)
                return BadRequest(new { mensaje = "Producto y bodega deben ser válidos." });

            if (datos.Stock != 0)
                return BadRequest(new
                {
                    mensaje = "La existencia debe crearse con stock 0. Registre una entrada para agregar unidades."
                });

            bool existeBodega = await _context.Bodegas
                .AnyAsync(b => b.IdBodega == datos.IdBodega);

            if (!existeBodega)
                return BadRequest(new { mensaje = "La bodega no existe." });

            bool duplicado = await _context.Existencias
                .AnyAsync(e => e.IdProducto == datos.IdProducto &&
                               e.IdBodega == datos.IdBodega);

            if (duplicado)
                return BadRequest(new
                {
                    mensaje = "Ya existe un registro para este producto en esta bodega."
                });

            var existencia = new Existencia
            {
                IdProducto = datos.IdProducto,
                IdBodega = datos.IdBodega,
                Stock = 0
            };

            _context.Existencias.Add(existencia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get),
                new { id = existencia.IdExistencia }, existencia);
        }

    
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Existencia datos)
        {
            var existencia = await _context.Existencias.FindAsync(id);

            if (existencia == null)
                return NotFound(new { mensaje = "Existencia no encontrada." });

            if (datos.IdProducto != existencia.IdProducto ||
                datos.IdBodega != existencia.IdBodega ||
                datos.Stock != existencia.Stock)
            {
                return BadRequest(new
                {
                    mensaje = "No se permite modificar directamente producto, bodega o stock. Use MovimientosInventario."
                });
            }

            return Ok(new { mensaje = "Registro validado. No se realizaron cambios." });
        }

   
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existencia = await _context.Existencias.FindAsync(id);

            if (existencia == null)
                return NotFound(new { mensaje = "Existencia no encontrada." });

            if (existencia.Stock != 0)
                return BadRequest(new
                {
                    mensaje = "No se puede eli  minar una existencia con stock."
                });

            bool tieneMovimientos = await _context.MovimientosInventario
                .AnyAsync(m => m.IdProducto == existencia.IdProducto &&
                               m.IdBodega == existencia.IdBodega);

            bool tieneReservas = await _context.ReservasStock
                .AnyAsync(r => r.IdProducto == existencia.IdProducto &&
                               r.IdBodega == existencia.IdBodega);

            if (tieneMovimientos || tieneReservas)
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar una existencia con historial de movimientos o reservas."
                });

            _context.Existencias.Remove(existencia);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Existencia eliminada correctamente." });
        }
    }
}
