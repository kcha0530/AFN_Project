# Backend Demo — Run Guide

A full-stack web app: **React + Vite** frontend, **ASP.NET Core** API, **PostgreSQL** database, all orchestrated by **.NET Aspire**.

---

## Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| .NET SDK | 10.0+ | https://dot.net |
| Node.js | 18+ | https://nodejs.org |
| Docker Desktop | Latest | https://docker.com |

Docker must be running before you start — Aspire uses it to launch PostgreSQL.

---

## Step-by-Step: Run the App

### 1. Navigate to the project root

```bash
cd D:\Krit\VSdef2
```

### 2. Install frontend dependencies

```bash
cd frontend
npm install
cd ..
```

### 3. Restore .NET dependencies

```bash
dotnet restore
```

### 4. Start the full stack with Aspire

```bash
cd backenddemo.AppHost
dotnet run
```

Or use the Aspire CLI:

```bash
aspire run
```

Aspire will automatically:
- Pull and start a **PostgreSQL** container
- Pull and start a **Redis** container
- Launch the **ASP.NET Core API** (with auto DB migration + seed data)
- Launch the **React frontend** dev server
- Open the **Aspire Dashboard** at `http://localhost:18024`

### 5. Access the app

| Service | URL |
|---------|-----|
| React Frontend | http://localhost:5173 |
| ASP.NET Core API | http://localhost:5474 |
| Swagger / OpenAPI | http://localhost:5474/swagger |
| Aspire Dashboard | http://localhost:18024 |
| PgAdmin (DB UI) | http://localhost:5050 |

### 6. Log in

Use the pre-seeded demo account:

```
Username: krit
Password: krit
```

---

## Stop the App

Press `Ctrl+C` in the terminal running Aspire. All containers shut down cleanly.

---

## Reset the Database

```bash
# 1. Stop Aspire (Ctrl+C)

# 2. Find and remove the PostgreSQL Docker volume
docker volume ls
docker volume rm <volume-name>

# 3. Restart
aspire run
```

The API will re-apply migrations and re-seed data on the next startup.

---

## Troubleshooting

**Port already in use:**
```bash
# Windows — find what's using port 5173
netstat -ano | findstr :5173
```

**Database won't connect:**
```bash
docker ps   # verify the postgres container is running
```

**Frontend can't reach API:**
- Open the Aspire Dashboard → check that `apiservice` shows as Running
- Check browser console for CORS errors
- Verify `VITE_API_BASE_URL` is set in the Aspire Dashboard environment tab

**Reset migrations manually:**
```bash
cd backenddemo.ApiService
dotnet ef database drop -f
dotnet ef database update
```

---

## Project Structure

```
VSdef2/
├── backenddemo.AppHost/          # Aspire orchestration — AppHost.cs wires everything
├── backenddemo.ApiService/       # ASP.NET Core Minimal API
│   ├── Program.cs                # All 19 endpoints
│   ├── Data/                     # EF Core DbContext
│   ├── Models/                   # Entities + DTOs
│   ├── Middleware/                # Security, logging, rate-limit middleware
│   └── Migrations/               # EF Core migrations
├── backenddemo.ServiceDefaults/  # Shared Aspire health-check + telemetry config
├── backenddemo.Web/              # Razor Pages project (Aspire default, not primary UI)
├── frontend/                     # React + Vite SPA
│   ├── src/
│   │   ├── App.jsx               # Root component + auth state
│   │   ├── components/
│   │   │   ├── LoginPage.jsx
│   │   │   ├── Dashboard.jsx
│   │   │   ├── BackendProducts.jsx
│   │   │   ├── ProductSearch.jsx
│   │   │   └── ...other components
│   └── vite.config.js            # Reads PORT env var from Aspire
└── backenddemo.sln
```
