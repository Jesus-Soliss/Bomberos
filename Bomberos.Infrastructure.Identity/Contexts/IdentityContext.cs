using Bomberos.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Infrastructure.Identity.Contexts;

public class IdentityContext : IdentityDbContext<ApplicationUser>
{
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.Nombre).HasMaxLength(50).IsUnicode(false).IsRequired();
            e.Property(u => u.Apellido).HasMaxLength(50).IsUnicode(false).IsRequired();
            e.Property(u => u.Estado).HasMaxLength(10).IsUnicode(false).IsRequired();
            e.ToTable(t => t.HasCheckConstraint("CK_AspNetUsers_Estado", "[Estado] IN ('ACTIVO', 'INACTIVO')"));
        });
    }
}
