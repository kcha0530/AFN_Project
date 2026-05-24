# Backend Demo Application - Complete Setup Guide

## 🚀 Quick Start

### Prerequisites
- **.NET 10 SDK** or later
- **Node.js 18+** with npm
- **Docker** (for PostgreSQL via Aspire)
- **Aspire CLI** (included with .NET 10)

### Step 1: Clone & Navigate
```bash
cd d:\Krit\VSdef2
```

### Step 2: Restore Dependencies
```bash
# Backend
dotnet restore

# Frontend
cd my-app
npm install
cd ..
```

### Step 3: Start with Aspire
```bash
dotnet workload install aspire
cd backenddemo.AppHost
aspire run
```

This will:
- ✅ Start PostgreSQL container
- ✅ Start Redis cache
- ✅ Start ASP.NET Core API service
- ✅ Start React Vite frontend (development server)
- ✅ Launch Aspire dashboard at `http://localhost:18024`

### Step 4: Access the Application
- **Frontend:** http://localhost:5173 (auto-opened)
- **Backend API:** http://localhost:5474
- **Swagger/OpenAPI:** http://localhost:5474/swagger
- **Aspire Dashboard:** http://localhost:18024 (monitoring & logs)
- **PostgreSQL Admin (PgAdmin):** http://localhost:5050

### Step 5: Login
Use demo credentials:
- **Username:** `krit`
- **Password:** `krit`

---

## 📁 Project Structure

```
backenddemo/
├── backenddemo.AppHost/           # Aspire orchestration
│   ├── AppHost.cs                 # Manages frontend, backend, database
│   └── backenddemo.AppHost.csproj
│
├── backenddemo.ApiService/        # ASP.NET Core minimal API
│   ├── Program.cs                 # API configuration & endpoints
│   ├── Data/
│   │   └── ApplicationDbContext.cs # EF Core DbContext
│   ├── Models/
│   │   ├── Product.cs
│   │   ├── User.cs
│   │   ├── AuthModels.cs
│   │   └── DTOs.cs
│   ├── Middleware/
│   │   └── SecurityMiddleware.cs   # Auth, logging, rate limiting
│   └── Migrations/                # Database migrations
│
├── backenddemo.Web/               # Razor pages (.NET) - not used in main flow
│
├── backenddemo.ServiceDefaults/   # Shared Aspire configuration
│   └── Extensions.cs              # Service discovery, health checks
│
├── my-app/                        # React SPA (Vite)
│   ├── index.html
│   ├── package.json
│   ├── src/
│   │   ├── App.jsx                # Main component
│   │   ├── main.jsx               # Entry point
│   │   └── components/
│   │       ├── LoginPage.jsx
│   │       ├── Dashboard.jsx
│   │       ├── BackendProducts.jsx
│   │       └── ...other components
│   └── .env.local                 # API_BASE_URL config
│
└── backenddemo.sln                # Solution file
```

---

## 🔐 Authentication Flow

```
1. User enters username & password in React LoginPage
   ↓
2. Frontend sends POST /auth/login to backend
   ↓
3. Backend queries PostgreSQL Users table
   ↓
4. Password verified with BCrypt hashing
   ↓
5. JWT token generated (valid for 60 minutes)
   ↓
6. Frontend stores token in localStorage
   ↓
7. All subsequent requests include: Authorization: Bearer <token>
```

---

## 💾 Database Overview

### PostgreSQL Tables

**Users Table**
```sql
- Id (Primary Key)
- Username (Unique, max 100 chars)
- Email (Unique, max 255 chars)
- PasswordHash (BCrypt hashed)
- FullName
- CreatedAt
- UpdatedAt
- IsActive (boolean)
```

**Products Table**
```sql
- Id (Primary Key)
- Name (max 255 chars)
- Price (decimal)
```

### Seeded Data
On first run, the backend automatically seeds:
- **Test User:** krit / krit (password: krit)
- **Test Products:** Keyboard ($50), Mouse ($30), Monitor ($299), USB-C Hub ($45)

---

## 🔌 API Architecture

### Technology Stack
| Component | Technology | Version |
|-----------|-----------|---------|
| Frontend | React + Vite | 19.2.6 + 8.0.12 |
| Backend | ASP.NET Core Minimal API | .NET 10.0 |
| Database | PostgreSQL | 15+ |
| Cache | Redis | Latest (via Aspire) |
| Orchestration | .NET Aspire | 13.3.3 |
| Authentication | JWT (HS256) | - |
| Password Hashing | BCrypt.Net | 4.0.3 |
| ORM | Entity Framework Core | 10.0 |
| API Docs | Swagger/OpenAPI | 10.0.7 |

---

## 🛑 Stopping & Restarting

### Stop Everything
```bash
# In the terminal where Aspire is running:
Ctrl+C
```

All containers and processes will cleanly shut down.

### Restart
```bash
aspire run
```

---

## 🧹 Clean Up Database

To reset all data:

```bash
# 1. Stop Aspire (Ctrl+C)

# 2. Remove PostgreSQL container volume
docker volume ls | grep postgres
docker volume rm <volume-name>

# 3. Restart Aspire
aspire run
```

---

## 📊 Monitoring & Logs

### Via Aspire Dashboard
Open http://localhost:18024 to see:
- Real-time resource status (frontend, backend, database, cache)
- Live logs from all services
- Trace information
- Performance metrics
- Health check results

### Via Command Line
```bash
# View Docker containers
docker ps

# View logs from API service
docker logs <container-id>

# PostgreSQL via Docker
docker exec -it <postgres-container-id> psql -U postgres -d demodb
```

---

## 🚨 Troubleshooting

### Port Already in Use
If you get "port 5173 is already in use":
```bash
# Find process using the port
netstat -ano | findstr :5173  # Windows
lsof -i :5173                 # macOS/Linux

# Kill process or change port in vite.config.js
```

### Database Connection Issues
```bash
# Check PostgreSQL is running
docker ps | grep postgres

# Test connection
psql -h localhost -U postgres -d demodb
```

### Migrations Failed
```bash
# Reset migrations
cd backenddemo.ApiService
dotnet ef database drop -f
dotnet ef database update

# Or clear Docker volume and restart Aspire
```

### Frontend can't reach API
- Check Aspire dashboard for API service status
- Verify VITE_API_BASE_URL in .env.local matches API endpoint
- Check browser console for CORS errors

---

## 📈 Next Steps

### Add More Features
1. Create new models in `backenddemo.ApiService/Models/`
2. Add DbSet to `ApplicationDbContext`
3. Create migration: `dotnet ef migrations add YourMigrationName`
4. Add endpoints in `Program.cs`
5. Update React components to call new endpoints

### Deploy to Production
- Update appsettings.json with production database connection
- Set JWT_KEY environment variable
- Use PostgreSQL managed service (Azure Database, AWS RDS, etc.)
- Deploy backend to container registry (Docker Hub, Azure Container Registry)
- Deploy frontend to static hosting (Vercel, Netlify, Azure Static Web Apps)

### Add More Security
- Implement role-based authorization
- Add HTTPS certificate pinning
- Implement refresh token rotation
- Add two-factor authentication
- Use secrets manager for sensitive configuration

---

## 📞 Support

For issues:
1. Check the **TROUBLESHOOTING.md** guide
2. Review **API_DOCUMENTATION.md** for endpoint details
3. Check Aspire dashboard for service health
4. Review backend logs in terminal or Aspire dashboard

---

**Last Updated:** May 25, 2026
