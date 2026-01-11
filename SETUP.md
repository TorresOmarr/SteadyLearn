# 🚀 SETUP.md - Getting Started Locally

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📌 Prerequisites

Before you start, ensure you have:

```
✓ Docker & Docker Compose (for PostgreSQL)
✓ .NET 8 SDK
✓ Node.js 18+ and pnpm
✓ Git
✓ A code editor (VS Code, Rider, etc.)
```

### Installation Commands

```bash
# macOS (using Homebrew)
brew install docker node git

# Ubuntu/Debian
sudo apt-get install docker.io docker-compose nodejs git

# Windows
# Download Docker Desktop, Node.js, and Git from their official websites
# Or use Chocolatey: choco install docker-desktop nodejs git

# Install pnpm globally
npm install -g pnpm
```

---

## 📁 Clone the Repository

```bash
cd ~/Projects  # or your preferred location
git clone https://github.com/your-org/SteadyLearn.git
cd SteadyLearn
```

---

## 🗄️ Database Setup

### Start PostgreSQL with Docker

```bash
# From the root directory
docker-compose up -d

# Verify PostgreSQL is running
docker ps

# You should see: postgres_steadylearn (or similar) running
```

### Connection Details
```
Host: localhost
Port: 5432
Database: steadylearn
Username: postgres
Password: postgres
```

### Check Database Connection
```bash
# Install psql if needed
# macOS: brew install postgresql
# Ubuntu: sudo apt-get install postgresql-client

# Connect to database
psql -h localhost -U postgres -d steadylearn

# List tables (should be empty initially)
\dt

# Exit
\q
```

---

## 🔧 Backend Setup

### Navigate to API Directory
```bash
cd api
```

### Restore NuGet Packages
```bash
dotnet restore
```

### Configure appsettings
Check `appsettings.Development.json`:

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
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Run Database Migrations
```bash
# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update

# Verify tables were created
# Connect to psql and run \dt
```

### Start the API
```bash
dotnet run

# API should be running at: https://localhost:5001
# Swagger documentation at: https://localhost:5001/swagger
```

### Verify API is Running
```bash
# In another terminal
curl -s https://localhost:5001/swagger | head -20

# Or open in browser
# https://localhost:5001/swagger
```

---

## 🎨 Frontend Setup

### Navigate to Client Directory
```bash
cd ../client
```

### Install Dependencies
```bash
pnpm install

# Or with npm if pnpm not available
npm install
```

### Create Environment File
Create `.env.local`:

```
VITE_API_URL=https://localhost:5001
VITE_ENV=development
```

### Start Development Server
```bash
pnpm dev

# Or with npm
npm run dev

# Frontend should be running at: http://localhost:5173
```

### Build for Production
```bash
pnpm build

# Built files in: dist/
```

---

## ✅ Verification Checklist

### Backend Checklist
- [ ] Docker: `docker ps` shows postgres running
- [ ] Database: `psql` connects successfully
- [ ] Migrations: `dotnet ef database update` completes without errors
- [ ] API: `dotnet run` starts without errors
- [ ] Swagger: `https://localhost:5001/swagger` loads
- [ ] Admin user: Seeded in database (admin@example.com)

### Frontend Checklist
- [ ] Dependencies: `pnpm install` completes successfully
- [ ] Dev server: `pnpm dev` starts without errors
- [ ] Hot reload: Changes reflect immediately
- [ ] Builds: `pnpm build` completes successfully
- [ ] No TypeScript errors: `pnpm lint` passes

---

## 🧪 Test the Full Stack

### Step 1: Login to Admin Account
1. Open `http://localhost:5173`
2. Click "Login"
3. Enter:
   - Email: `admin@example.com`
   - Password: `AdminPassword123!`
4. You should be redirected to dashboard

### Step 2: Create a Course (via API)
```bash
# Get admin access token via login endpoint
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "AdminPassword123!"
  }'

# Response will include accessToken
# Copy the token

# Create a course
curl -X POST https://localhost:5001/api/courses \
  -H "Authorization: Bearer <YOUR_ACCESS_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "titleEn": "Web Development Basics",
    "titleEs": "Fundamentos de Desarrollo Web",
    "descriptionEn": "Learn the basics of web development",
    "descriptionEs": "Aprende los fundamentos del desarrollo web"
  }'

# Response should include the created course with ID
```

### Step 3: View Course in Frontend
1. Refresh the frontend (http://localhost:5173)
2. You should see the created course in the course list
3. Click on the course to open the builder

---

## 🔄 Common Development Workflows

### Run Backend with Hot Reload
```bash
cd api
dotnet watch run

# Changes to code automatically restart the app
```

### Run Frontend with Hot Reload
```bash
cd client
pnpm dev

# Changes to code automatically reload in browser
```

### Reset Database
```bash
# Stop the database
docker-compose down

# Remove volume (optional, to clear data)
docker volume rm steadylearn_postgres_data

# Start fresh
docker-compose up -d

# Re-run migrations
cd api
dotnet ef database update
```

### Run Tests (Backend)
```bash
cd api
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

### Run Linter (Frontend)
```bash
cd client
pnpm lint

# Fix issues automatically
pnpm lint --fix
```

---

## 🐛 Troubleshooting

### PostgreSQL Connection Failed
```bash
# Check if container is running
docker ps

# If not running
docker-compose up -d

# Check logs
docker logs steadylearn-postgres-1

# Or use container name from docker ps output
```

### Port Already in Use
```bash
# Backend (5001)
sudo lsof -i :5001
kill -9 <PID>

# Frontend (5173)
sudo lsof -i :5173
kill -9 <PID>

# Database (5432)
sudo lsof -i :5432
kill -9 <PID>
```

### TypeScript Errors in Frontend
```bash
# Clear node_modules and reinstall
cd client
rm -rf node_modules pnpm-lock.yaml
pnpm install
```

### Migration Issues
```bash
# Remove last migration
dotnet ef migrations remove

# Start over
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### CORS Errors
Ensure API is running on `https://localhost:5001` and frontend on `http://localhost:5173`.
Check CORS middleware is configured in `Program.cs`:

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

app.UseCors("Development");
```

---

## 📚 Useful Commands Reference

### Backend
```bash
# Navigate to api directory
cd api

# Restore packages
dotnet restore

# Build project
dotnet build

# Run project
dotnet run

# Run with hot reload
dotnet watch run

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# View database (psql)
psql -h localhost -U postgres -d steadylearn
```

### Frontend
```bash
# Navigate to client directory
cd client

# Install dependencies
pnpm install

# Start dev server
pnpm dev

# Build for production
pnpm build

# Preview production build
pnpm preview

# Lint code
pnpm lint

# Format code
pnpm format
```

### Docker
```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f postgres

# Connect to database
docker exec -it steadylearn-postgres-1 psql -U postgres
```

---

## 🔗 Documentation Links

- **AGENTS.md** - Blueprint and decisions
- **ARCHITECTURE.md** - Code structure
- **API_DESIGN.md** - API endpoints
- **AUTH_IMPLEMENTATION.md** - Auth system
- **TESTING.md** - Testing approach

---

## 📞 Getting Help

### Check Logs
```bash
# API logs (console output)
# Frontend logs (browser console)
# Database logs: docker logs <container_name>
```

### Common Issues
1. **API doesn't start**: Check PostgreSQL is running
2. **Database migrations fail**: Ensure connection string is correct
3. **Frontend can't reach API**: Check CORS configuration
4. **Port conflicts**: Use different port in appsettings or vite.config.ts

---

## 🚀 Next Steps

After setup:
1. Read AGENTS.md for architecture overview
2. Explore ARCHITECTURE.md for code structure
3. Review API_DESIGN.md for endpoint contracts
4. Start with AUTH implementation (Sprint 1)

---

*A good setup is the foundation of productive development.*
