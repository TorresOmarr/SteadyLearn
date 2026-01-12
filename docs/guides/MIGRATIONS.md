# Database Migrations Guide

# Solo humano crea/aplica migraciones
(No las genera ni aplica un agente. Si se necesita una nueva migración, pedirla al humano.)

## Prerequisites

1. Install EF Core CLI tools:
   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.0
   ```

2. Ensure PostgreSQL is running:
   ```bash
   docker-compose up -d
   ```

## Creating Migrations

### Initial Migration

```bash
cd api
dotnet ef migrations add InitialCreate --output-dir src/Data/Migrations
```

Creates:
- `{timestamp}_InitialCreate.cs` - Migration file
- `{timestamp}_InitialCreate.Designer.cs` - Designer file
- `ApplicationDbContextModelSnapshot.cs` - Model snapshot

### Adding New Entities

```bash
dotnet ef migrations add Add{EntityName}Table
```

### Modifying Entities

```bash
dotnet ef migrations add Update{EntityName}{Description}
```

## Applying Migrations

**Option 1: Automatic (on app startup)**
```csharp
// Program.cs
await context.Database.MigrateAsync();
```

**Option 2: Manual**
```bash
dotnet ef database update
```

## Rolling Back

```bash
# Rollback to specific migration
dotnet ef database update <MigrationName>

# Rollback all
dotnet ef database update 0
```

## Removing Migrations

```bash
# Only if NOT applied yet
dotnet ef migrations remove
```

## Workflow

### Adding a New Entity

1. Create entity in `src/Domain/Entities/`
2. Add DbSet to `ApplicationDbContext.cs`
3. Create configuration in `src/Data/Configurations/`
4. Apply configuration in `OnModelCreating()`
5. Create migration: `dotnet ef migrations add Add{Entity}Table`
6. Review generated file
7. Apply: `dotnet ef database update`

## Useful Commands

```bash
# List all migrations
dotnet ef migrations list

# Generate SQL script
dotnet ef migrations script

# Generate script between migrations
dotnet ef migrations script <From> <To>

# Drop database (destructive!)
dotnet ef database drop
```

## Troubleshooting

### "Build failed"
```bash
dotnet build  # Fix build errors first
```

### "No DbContext found"
```bash
cd api  # Must be in project directory
```

### "Connection string not found"
- Check `appsettings.json` has `ConnectionStrings:DefaultConnection`
- Verify PostgreSQL is running: `docker ps`

## Best Practices

1. **Always review migrations** before applying
2. **Use descriptive names**: `AddCourseTable`, `AddPublishedFlagToCourse`
3. **Don't edit applied migrations** - create new ones
4. **Keep migrations small** - one logical change each
5. **Test locally** before production
6. **Backup production** before applying

## Current Schema

After initial migration:

- **Users** - Authentication, tokens, audit fields
- Future: Courses, Topics, Subtopics, Sessions, Translations

## Related Documentation

- [Database](../backend/DATABASE.md) - Schema details
- [Domain Model](../backend/DOMAIN.md) - Entity definitions
