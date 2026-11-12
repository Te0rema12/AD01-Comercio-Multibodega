
using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly CatalogoDbContext _context;

        public ProductosController(CatalogoDbContext context)
        {
            _context = context;
        }

     
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .ToListAsync();

            return Ok(productos);
        }

     
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound("El producto no existe.");

            return Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre) ||
                string.IsNullOrWhiteSpace(producto.SKU))
                return BadRequest("Nombre y SKU son obligatorios.");

            if (producto.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            bool categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == producto.IdCategoria);

            if (!categoriaExiste)
                return BadRequest("La categoría no existe.");

            bool marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == producto.IdMarca);

            if (!marcaExiste)
                return BadRequest("La marca no existe.");

            bool skuExiste = await _context.Productos
                .AnyAsync(p => p.SKU == producto.SKU);

            if (skuExiste)
                return Conflict("El SKU ya está registrado.");

            producto.IdProducto = 0;
            producto.Categoria = null;
            producto.Marca = null;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Get),
                new { id = producto.IdProducto },
                producto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Producto producto)
        {
            if (id != producto.IdProducto)
                return BadRequest("El ID no coincide.");

            if (string.IsNullOrWhiteSpace(producto.Nombre) ||
                string.IsNullOrWhiteSpace(producto.SKU))
                return BadRequest("Nombre y SKU son obligatorios.");

            if (producto.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            var existente = await _context.Productos.FindAsync(id);

            if (existente == null)
                return NotFound("El producto no existe.");

            bool categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == producto.IdCategoria);

            bool marcaExiste = await _context.Marcas
                .AnyAsync(m => m.IdMarca == producto.IdMarca);

            if (!categoriaExiste || !marcaExiste)
                return BadRequest("La categoría o marca no existe.");

            bool skuDuplicado = await _context.Productos
                .AnyAsync(p => p.SKU == producto.SKU && p.IdProducto != id);

            if (skuDuplicado)
                return Conflict("El SKU ya pertenece a otro producto.");

            existente.IdCategoria = producto.IdCategoria;
            existente.IdMarca = producto.IdMarca;
            existente.Nombre = producto.Nombre;
            existente.SKU = producto.SKU;
            existente.Precio = producto.Precio;
            existente.Estado = producto.Estado;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound("El producto no existe.");

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
