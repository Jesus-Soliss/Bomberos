using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bomberos.Infrastructure.Persistence.Contexts;

public class ApplicationContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    public ApplicationContext(DbContextOptions<ApplicationContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Estacion> Estaciones => Set<Estacion>();
    public DbSet<Bombero> Bomberos => Set<Bombero>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<Emergencia> Emergencias => Set<Emergencia>();
    public DbSet<AsignacionBombero> AsignacionesBomberos => Set<AsignacionBombero>();
    public DbSet<AsignacionVehiculo> AsignacionesVehiculos => Set<AsignacionVehiculo>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Estacion>(e =>
        {
            e.ToTable("Estacion");
            e.Property(x => x.Nombre).Varchar(100);
            e.Property(x => x.Direccion).Varchar(200);
            e.Property(x => x.Telefono).Varchar(15);
            e.Property(x => x.Estado).EnumTexto(10);
            e.HasIndex(x => x.Nombre).IsUnique();
        });

        mb.Entity<Bombero>(e =>
        {
            e.ToTable("Bombero");
            e.Property(x => x.Nombre).Varchar(50);
            e.Property(x => x.Apellido).Varchar(50);
            e.Property(x => x.Cedula).Varchar(15);
            e.Property(x => x.Rango).EnumTexto(10);
            e.Property(x => x.Estado).EnumTexto(10);
            e.HasIndex(x => x.Cedula).IsUnique();
            e.HasOne(x => x.Estacion).WithMany(x => x.Bomberos)
                .HasForeignKey(x => x.EstacionId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Vehiculo>(e =>
        {
            e.ToTable("Vehiculo");
            e.Property(x => x.Placa).Varchar(10);
            e.Property(x => x.Tipo).EnumTexto(10);
            e.Property(x => x.Estado).EnumTexto(20);
            e.HasIndex(x => x.Placa).IsUnique();
            e.HasOne(x => x.Estacion).WithMany(x => x.Vehiculos)
                .HasForeignKey(x => x.EstacionId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Emergencia>(e =>
        {
            e.ToTable("Emergencia");
            e.Property(x => x.Tipo).EnumTexto(30);
            e.Property(x => x.Descripcion).Varchar(500);
            e.Property(x => x.Direccion).Varchar(200);
            e.Property(x => x.Prioridad).EnumTexto(15);
            // El documento indica VARCHAR(10), pero EN_ATENCION tiene 11 caracteres.
            e.Property(x => x.Estado).EnumTexto(15);
            e.Property(x => x.Origen).EnumTexto(20);
            e.Property(x => x.ReferenciaExterna).Varchar(50);
            e.HasIndex(x => x.ReferenciaExterna).IsUnique().HasFilter("[ReferenciaExterna] IS NOT NULL");
        });

        mb.Entity<AsignacionBombero>(e =>
        {
            e.ToTable("AsignacionBombero");
            e.HasKey(x => new { x.EmergenciaId, x.BomberoId });
            e.Property(x => x.Estado).EnumTexto(15);
            e.HasOne(x => x.Emergencia).WithMany(x => x.AsignacionesBomberos)
                .HasForeignKey(x => x.EmergenciaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Bombero).WithMany(x => x.Asignaciones)
                .HasForeignKey(x => x.BomberoId).OnDelete(DeleteBehavior.Restrict);
            // RN-03 garantizada también por la base de datos.
            e.HasIndex(x => x.BomberoId).IsUnique().HasFilter("[Estado] = 'ACTIVA'")
                .HasDatabaseName("UX_AsignacionBombero_Activa");
        });

        mb.Entity<AsignacionVehiculo>(e =>
        {
            e.ToTable("AsignacionVehiculo");
            e.HasKey(x => new { x.EmergenciaId, x.VehiculoId });
            e.Property(x => x.Estado).EnumTexto(15);
            e.HasOne(x => x.Emergencia).WithMany(x => x.AsignacionesVehiculos)
                .HasForeignKey(x => x.EmergenciaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Vehiculo).WithMany(x => x.Asignaciones)
                .HasForeignKey(x => x.VehiculoId).OnDelete(DeleteBehavior.Restrict);
            // RN-04 garantizada también por la base de datos.
            e.HasIndex(x => x.VehiculoId).IsUnique().HasFilter("[Estado] = 'ACTIVA'")
                .HasDatabaseName("UX_AsignacionVehiculo_Activa");
        });

        mb.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLog");
            e.Property(x => x.UsuarioId).Varchar(450);
            e.Property(x => x.Accion).Varchar(100);
            e.Property(x => x.Entidad).Varchar(50);
            e.Property(x => x.Resultado).EnumTexto(20);
            e.HasIndex(x => x.FechaHora);
        });

        mb.Entity<RequestLog>(e =>
        {
            e.ToTable("RequestLog");
            e.Property(x => x.UsuarioId).Varchar(450);
            e.Property(x => x.Metodo).Varchar(10);
            e.Property(x => x.Endpoint).Varchar(300);
            e.HasIndex(x => x.FechaHora);
        });

        foreach (var auditable in mb.Model.GetEntityTypes().Where(t => typeof(AuditableBaseEntity).IsAssignableFrom(t.ClrType)))
        {
            mb.Entity(auditable.ClrType).Property(nameof(AuditableBaseEntity.CreatedBy)).HasMaxLength(450).IsUnicode(false);
            mb.Entity(auditable.ClrType).Property(nameof(AuditableBaseEntity.LastModifiedBy)).HasMaxLength(450).IsUnicode(false);
        }

        base.OnModelCreating(mb);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<AuditableBaseEntity>())
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.Created = DateTime.UtcNow;
                e.Entity.CreatedBy = _currentUser?.UserId;
            }
            if (e.State == EntityState.Modified)
            {
                e.Entity.LastModified = DateTime.UtcNow;
                e.Entity.LastModifiedBy = _currentUser?.UserId;
            }
        }
        return base.SaveChangesAsync(ct);
    }
}

internal static class PropertyBuilderExtensions
{
    public static PropertyBuilder<T> Varchar<T>(this PropertyBuilder<T> property, int longitud)
        => property.HasMaxLength(longitud).IsUnicode(false);

    /// <summary>Guarda el enum como el texto del documento funcional (EN_SERVICIO, CENTRAL_911, ...).</summary>
    public static PropertyBuilder<TEnum> EnumTexto<TEnum>(this PropertyBuilder<TEnum> property, int longitud)
        where TEnum : struct, Enum
        => property
            .HasConversion(new ValueConverter<TEnum, string>(
                v => Core.Domain.Common.EnumTexto.ToTextoValido(v),
                s => Core.Domain.Common.EnumTexto.Parse<TEnum>(s)))
            .HasMaxLength(longitud)
            .IsUnicode(false);
}
