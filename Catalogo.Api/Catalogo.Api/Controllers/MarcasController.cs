using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcasController : ControllerBase
    {
        private readonly CatalogoDbContext _context;

        public MarcasController(CatalogoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> Get()
        {
            return Ok(await _context.Marcas.AsNoTracking().ToListAsync());
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Marca>> Get(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
                return NotFound("La marca no existe.");

            return Ok(marca);
        }


        [HttpPost]
        public async Task<ActionResult<Marca>> Post(Marca marca)
        {
            if (string.IsNullOrWhiteSpace(marca.Nombre))
                return BadRequest("El nombre es obligatorio.");

            marca.IdMarca = 0;

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = marca.IdMarca },
                marca);
        }

   
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Marca marca)
        {
            if (id != marca.IdMarca)
                return BadRequest("El ID no coincide.");

            if (string.IsNullOrWhiteSpace(marca.Nombre))
                return BadRequest("El nombre es obligatorio.");

            if (!await _context.Marcas.AnyAsync(m => m.IdMarca == id))
                return NotFound("La marca no existe.");

            _context.Entry(marca).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound("La marca ya no existe.");
            }

            return NoContent();
        }

 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
                return NotFound("La marca no existe.");

            bool tieneProductos = await _context.Productos
                .AnyAsync(p => p.IdMarca == id);

            if (tieneProductos)
                return Conflict("No se puede eliminar: tiene productos relacionados.");

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
