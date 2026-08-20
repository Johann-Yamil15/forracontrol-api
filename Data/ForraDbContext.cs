using ForraControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Data;

public class ForraDbContext(DbContextOptions<ForraDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Presentacion> Presentaciones => Set<Presentacion>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<PrecioEspecial> PreciosEspeciales => Set<PrecioEspecial>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Usuarios ─────────────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(e =>
        {
            // DateTime.Now (hora local) igual que el API viejo — sin zona horaria en la columna.
            e.Property(u => u.CreatedAt).HasColumnType("timestamp without time zone");
            e.HasIndex(u => u.Username).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("chk_usuarios_rol", "rol IN ('admin', 'trabajador')"));
            e.HasMany(u => u.Ventas)
                .WithOne(v => v.Usuario)
                .HasForeignKey(v => v.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Clientes ─────────────────────────────────────────────────────
        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.CreatedAt).HasColumnType("timestamp without time zone");
        });

        // ── Presentaciones ───────────────────────────────────────────────
        modelBuilder.Entity<Presentacion>(e =>
        {
            e.Property(p => p.Tamano).HasPrecision(10, 2);
            e.Property(p => p.Precio).HasPrecision(10, 2);
            e.Property(p => p.PrecioCosto).HasPrecision(10, 2);

            e.HasOne(p => p.Producto)
                .WithMany(p => p.Presentaciones)
                .HasForeignKey(p => p.IdProducto)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(p => p.IdProducto).HasDatabaseName("ix_presentaciones_producto");
            e.HasIndex(p => new { p.Stock, p.StockMinimo })
                .HasDatabaseName("ix_presentaciones_stock")
                .HasFilter("activo = true");

            e.ToTable(t =>
            {
                t.HasCheckConstraint("chk_presentaciones_precio", "precio >= 0");
                t.HasCheckConstraint("chk_presentaciones_stock", "stock >= 0");
                t.HasCheckConstraint("chk_presentaciones_stock_minimo", "stock_minimo >= 0");
            });
        });

        // ── PreciosEspeciales ────────────────────────────────────────────
        modelBuilder.Entity<PrecioEspecial>(e =>
        {
            e.Property(p => p.Precio).HasColumnName("precio_especial").HasPrecision(10, 2);

            e.HasOne(p => p.Cliente)
                .WithMany(c => c.PreciosEspeciales)
                .HasForeignKey(p => p.IdCliente)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Presentacion)
                .WithMany(pr => pr.PreciosEspeciales)
                .HasForeignKey(p => p.IdPresentacion)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(p => new { p.IdCliente, p.IdPresentacion })
                .IsUnique()
                .HasDatabaseName("uq_precios_especiales");

            e.ToTable(t => t.HasCheckConstraint("chk_precios_especiales_precio", "precio_especial >= 0"));
        });

        // ── Ventas ───────────────────────────────────────────────────────
        modelBuilder.Entity<Venta>(e =>
        {
            e.Property(v => v.Fecha).HasColumnType("timestamp without time zone");
            e.Property(v => v.TotalOriginal).HasPrecision(10, 2);
            e.Property(v => v.Descuento).HasPrecision(10, 2);
            e.Property(v => v.TotalFinal).HasPrecision(10, 2);

            e.HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(v => v.Fecha).IsDescending().HasDatabaseName("ix_ventas_fecha");
            e.HasIndex(v => v.IdUsuario).HasDatabaseName("ix_ventas_usuario");
            e.HasIndex(v => v.IdCliente).HasDatabaseName("ix_ventas_cliente");

            e.ToTable(t =>
            {
                t.HasCheckConstraint("chk_ventas_descuento", "descuento >= 0");
                t.HasCheckConstraint("chk_ventas_total_final", "total_final >= 0");
            });
        });

        // ── DetalleVenta ─────────────────────────────────────────────────
        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.Property(d => d.PrecioUnitario).HasPrecision(10, 2);
            e.Property(d => d.PrecioEfectivo).HasPrecision(10, 2);
            e.Property(d => d.Subtotal).HasPrecision(10, 2);
            e.Property(d => d.PrecioCosto).HasPrecision(10, 2);

            e.HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Presentacion)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.IdPresentacion)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(d => d.IdVenta).HasDatabaseName("ix_detalle_ventas_venta");

            e.ToTable(t => t.HasCheckConstraint("chk_detalle_ventas_cantidad", "cantidad > 0"));
        });
    }
}
