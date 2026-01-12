# Setup Guide

## Prerequisites

```
- Docker & Docker Compose (for PostgreSQL)
- .NET 8 SDK
- Node.js 18+ and pnpm
- Git
- Code editor (VS Code, Rider, etc.)
```

### Installation

```bash
# macOS (Homebrew)
brew install docker node git

# Ubuntu/Debian
sudo apt-get install docker.io docker-compose nodejs git

# Windows
# Use Chocolatey: choco install docker-desktop nodejs git

# Install pnpm
npm install -g pnpm
```

## Quick Start

```bash
# 1. Clone repository
git clone https://github.com/your-org/SteadyLearn.git
cd SteadyLearn

# 2. Start PostgreSQL
docker-compose up -d

# 3. Run backend
cd api
dotnet restore
dotnet ef database update
dotnet run

# 4. Run frontend (new terminal)
cd client
pnpm install
pnpm dev
```

## Database Setup

### Start PostgreSQL

```bash
docker-compose up -d
docker ps  # Verify container is running
```

### Connection Details

```
Host: localhost
Port: 5432
Database: steadylearn
Username: postgres
Password: postgres
```

### Verify Connection

```bash
psql -h localhost -U postgres -d steadylearn
\dt  # List tables
\q   # Exit
```

## Backend Setup

### Configure appsettings

Check `api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=steadylearn;User Id=postgres;Password=postgres;"
  },
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-long",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Admin": {
    "Email": "admin@example.com",
    "Password": "AdminPassword123!",
    "FirstName": "Admin",
    "LastName": "User"
  }
}
```

### Run Migrations & Start

```bash
cd api
dotnet restore
dotnet ef database update
dotnet run
```

API available at: `https://localhost:5001`
Swagger: `https://localhost:5001/swagger`

## Frontend Setup

```bash
cd client
pnpm install
```

Create `.env.local`:
```
VITE_API_URL=https://localhost:5001
VITE_ENV=development
```

Start dev server:
```bash
pnpm dev
```

Frontend available at: `http://localhost:5173`

## Verification Checklist

### Backend
- [ ] Docker: `docker ps` shows postgres running
- [ ] Migrations: `dotnet ef database update` completes
- [ ] API: `dotnet run` starts without errors
- [ ] Swagger: `https://localhost:5001/swagger` loads
- [ ] Admin user seeded (admin@example.com)

### Frontend
- [ ] Dependencies: `pnpm install` completes
- [ ] Dev server: `pnpm dev` starts
- [ ] No TypeScript errors: `pnpm lint` passes

## Test Full Stack

1. Open `http://localhost:5173`
2. Login with:
   - Email: `admin@example.com`
   - Password: `AdminPassword123!`
3. You should see the dashboard

## Common Commands

### Backend

```bash
cd api
dotnet restore          # Restore packages
dotnet build            # Build project
dotnet run              # Run project
dotnet watch run        # Run with hot reload
dotnet test             # Run tests
dotnet ef migrations add <Name>   # Create migration
dotnet ef database update         # Apply migrations
```

### Frontend

```bash
cd client
pnpm install    # Install dependencies
pnpm dev        # Start dev server
pnpm build      # Build for production
pnpm preview    # Preview production build
pnpm lint       # Lint code
```

### Docker

```bash
docker-compose up -d      # Start services
docker-compose down       # Stop services
docker-compose logs -f    # View logs
```

## Troubleshooting

### PostgreSQL Connection Failed

```bash
docker ps                    # Check if running
docker-compose up -d         # Start if not
docker logs <container>      # Check logs
```

### Port Already in Use

```bash
sudo lsof -i :5001    # Find process on port
kill -9 <PID>         # Kill it
```

### Reset Database

```bash
docker-compose down
docker volume rm steadylearn_postgres_data
docker-compose up -d
cd api && dotnet ef database update
```

### CORS Errors

Ensure CORS is configured in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Related Documentation

- [Project Overview](../project/OVERVIEW.md) - Vision and scope
- [Migrations Guide](./MIGRATIONS.md) - Database migrations
- [Backend Architecture](../backend/ARCHITECTURE.md) - Code structure
