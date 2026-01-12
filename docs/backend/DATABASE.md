
## Overview

- **ORM**: Entity Framework Core 8
- **Database**: PostgreSQL 15
- **Migrations**: Code-First approach (solo humano las ejecuta/crea)
- **Strategy**: Soft delete + Auditing + Translations
- **Timestamps**: `DateTimeOffset` → `timestamp with time zone`

## Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=steadylearn;User Id=postgres;Password=postgres;"
  }
}
```

## Refresh Tokens (seguridad)

- Tabla dedicada `RefreshTokens` (hash + familia + estado).
- Estados: `Active`, `Used`, `Revoked`, `Expired`.
- Rotación: se marca el token anterior como `Used`, se crea uno nuevo en la misma familia; si se detecta reutilización de un token no activo, se revoca toda la familia.
- Límite: 1 activo + 5 históricos por usuario; los históricos extra se eliminan (hard delete) al rotar.
- Logout / reset password: se revocan todas las familias del usuario.
- Persistencia: `DateTimeOffset` → `timestamp with time zone`.

## ApplicationDbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}
```

## Entity Configuration Pattern

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.Role).HasConversion<int>();
        builder.Property(x => x.IsEmailVerified).HasDefaultValue(false);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
```

## Soft Delete

Todos los queries respetan `IsDeleted = false`.

```csharp
course.IsDeleted = true;
course.UpdatedAt = DateTimeOffset.UtcNow;
await _db.SaveChangesAsync();
```

## Database Seeding

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await context.SeedAdminAsync(builder.Configuration);
}
```

**AdminSeeder**:
```csharp
var admin = User.Create(
    config["Admin:Email"],
    BCrypt.Net.BCrypt.HashPassword(config["Admin:Password"]),
    config["Admin:FirstName"],
    config["Admin:LastName"],
    UserRole.Admin);
admin.MarkEmailAsVerified();
```

## Checklist

- [ ] Todas las entidades con `IEntityTypeConfiguration`
- [ ] Llaves foráneas definidas
- [ ] Índices en columnas críticas
- [ ] Únicos donde aplique
- [ ] `RefreshTokens` con límites y rotación segura
