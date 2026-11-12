
using Inventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Data
{
    public class InventarioDbContext : DbContext
    {
        public InventarioDbContext(
            DbContextOptions<InventarioDbContext> options
        ) : base(options)
        {
        }

        public DbSet<Bodega> Bodegas { get; set; }
        public DbSet<Existencia> Existencias { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<ReservaStock> ReservasStock { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Existencia>()
                .HasOne(e => e.Bodega)
                .WithMany()
                .HasForeignKey(e => e.IdBodega)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Bodega)
                .WithMany()
                .HasForeignKey(m => m.IdBodega)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReservaStock>()
                .HasOne(r => r.Bodega)
                .WithMany()
                .HasForeignKey(r => r.IdBodega)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
