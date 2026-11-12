
using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly CatalogoDbContext _context;

        public CategoriasController(CatalogoDbContext context)
        {
            _context = context;
        }

 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> Get()
        {
            return Ok(await _context.Categorias.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> Get(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound("La categoría no existe.");

            return Ok(categoria);
        }

 
        [HttpPost]
        public async Task<ActionResult<Categoria>> Post(Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                return BadRequest("El nombre es obligatorio.");

            categoria.IdCategoria = 0;

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = categoria.IdCategoria },
                categoria);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Categoria categoria)
        {
            if (id != categoria.IdCategoria)
                return BadRequest("El ID no coincide.");

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                return BadRequest("El nombre es obligatorio.");

            if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == id))
                return NotFound("La categoría no existe.");

            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound("La categoría ya no existe.");
            }

            return NoContent();
        }

  
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound("La categoría no existe.");

            bool tieneProductos = await _context.Productos
                .AnyAsync(p => p.IdCategoria == id);

            if (tieneProductos)
                return Conflict("No se puede eliminar: tiene productos relacionados.");

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

