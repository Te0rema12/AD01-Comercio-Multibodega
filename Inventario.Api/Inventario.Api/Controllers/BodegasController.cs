
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BodegasController : ControllerBase
    {
        private readonly InventarioDbContext _context;

        public BodegasController(InventarioDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bodega>>> Get()
        {
            return Ok(await _context.Bodegas
                .AsNoTracking()
                .ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Bodega>> Get(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id);

            if (bodega == null)
                return NotFound(new { mensaje = "Bodega no encontrada." });

            return Ok(bodega);
        }

    
        [HttpPost]
        public async Task<ActionResult<Bodega>> Post(Bodega bodega)
        {
            if (string.IsNullOrWhiteSpace(bodega.Nombre) ||
                string.IsNullOrWhiteSpace(bodega.Ciudad))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre y la ciudad son obligatorios."
                });
            }

            bodega.Nombre = bodega.Nombre.Trim();
            bodega.Ciudad = bodega.Ciudad.Trim();

            _context.Bodegas.Add(bodega);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = bodega.IdBodega },
                bodega
            );
        }

      
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Bodega datos)
        {
            var bodega = await _context.Bodegas.FindAsync(id);

            if (bodega == null)
                return NotFound(new { mensaje = "Bodega no encontrada." });

            if (string.IsNullOrWhiteSpace(datos.Nombre) ||
                string.IsNullOrWhiteSpace(datos.Ciudad))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre y la ciudad son obligatorios."
                });
            }

            bodega.Nombre = datos.Nombre.Trim();
            bodega.Ciudad = datos.Ciudad.Trim();
            bodega.Direccion = datos.Direccion;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Bodega actualizada correctamente." });
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id);

            if (bodega == null)
                return NotFound(new { mensaje = "Bodega no encontrada." });

            bool tieneExistencias = await _context.Existencias
                .AnyAsync(e => e.IdBodega == id);

            bool tieneMovimientos = await _context.MovimientosInventario
                .AnyAsync(m => m.IdBodega == id);

            bool tieneReservas = await _context.ReservasStock
                .AnyAsync(r => r.IdBodega == id);

            if (tieneExistencias || tieneMovimientos || tieneReservas)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede eliminar la bodega porque tiene registros relacionados."
                });
            }

            _context.Bodegas.Remove(bodega);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Bodega eliminada correctamente." });
        }
    }
}
