using Microsoft.EntityFrameworkCore;

namespace Bomberos.Integration.Api.Data;

public class IntegrationContext : DbContext
{
    public IntegrationContext(DbContextOptions<IntegrationContext> options) : base(options) { }

    public DbSet<IntegrationLog> IntegrationLogs => Set<IntegrationLog>();
    public DbSet<OperacionPendiente> OperacionesPendientes => Set<OperacionPendiente>();
    public DbSet<CacheRecurso> CacheRecursos => Set<CacheRecurso>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<IntegrationLog>(e =>
        {
            e.ToTable("IntegrationLog");
            e.Property(x => x.Origen).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.Metodo).HasMaxLength(10).IsUnicode(false);
            e.Property(x => x.Endpoint).HasMaxLength(300).IsUnicode(false);
            e.HasIndex(x => x.FechaHora);
        });

        mb.Entity<OperacionPendiente>(e =>
        {
            e.ToTable("OperacionPendiente");
            e.Property(x => x.UsuarioId).HasMaxLength(450).IsUnicode(false);
            e.Property(x => x.Tipo).HasMaxLength(40).IsUnicode(false);
            e.Property(x => x.Metodo).HasMaxLength(10).IsUnicode(false);
            e.Property(x => x.Endpoint).HasMaxLength(300).IsUnicode(false);
            e.Property(x => x.ReferenciaProvisional).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.Estado).HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.Motivo).HasMaxLength(500);
            e.Property(x => x.Reintentos).HasDefaultValue(0);
            e.HasIndex(x => x.Estado);
        });

        mb.Entity<CacheRecurso>(e =>
        {
            e.ToTable("CacheRecurso");
            e.Property(x => x.Tipo).HasMaxLength(10).IsUnicode(false);
            e.Property(x => x.Descripcion).HasMaxLength(150).IsUnicode(false);
            e.Property(x => x.Estado).HasMaxLength(20).IsUnicode(false);
            e.HasIndex(x => new { x.Tipo, x.ExternalId }).IsUnique();
        });
    }
}
