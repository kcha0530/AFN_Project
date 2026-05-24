# Project Summary

## Languages

| Layer | Language |
|-------|---------|
| Frontend | JavaScript (JSX / ES Modules) |
| Backend | C# (.NET 10) |
| Database | SQL (PostgreSQL dialect via EF Core migrations) |
| Config / Infra | JSON (appsettings, package.json) |

---

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Frontend framework | React | 19.2.6 |
| Frontend build tool | Vite | 8.0.12 |
| Backend framework | ASP.NET Core Minimal API | .NET 10 |
| ORM | Entity Framework Core + Npgsql | 10.0 / 10.0.1 |
| Database | PostgreSQL | 15+ (via Docker / Aspire) |
| Cache | Redis | Latest (via Docker / Aspire) |
| Orchestration | .NET Aspire | 13.3.3 |
| Authentication | JWT Bearer (HS256) | ASP.NET Core 10 |
| Password hashing | BCrypt.Net-Next | 4.0.3 |
| API documentation | Swagger / Swashbuckle | 10.1.7 |
| Container runtime | Docker Desktop | Latest |

---

## Architecture

```
[ React Frontend ]  <-- VITE_API_BASE_URL (injected by Aspire)
         |
         |  HTTP + JWT Bearer
         v
[ ASP.NET Core Minimal API ]
         |
         +-- Entity Framework Core (ORM)
         |
         v
[ PostgreSQL ]   <- managed in Docker by .NET Aspire

[ Redis ]        <- managed in Docker by .NET Aspire (output cache)

[ .NET Aspire ]  <- orchestrates all services, provides dashboard + telemetry
```

**Pattern:** Monorepo, single-solution. Frontend and backend live in the same Git repository and are wired together through the Aspire AppHost.

**API style:** REST, Minimal APIs (no controllers). All endpoints defined in `Program.cs`.

**Auth flow:** JWT issued on login → stored in `localStorage` → sent as `Authorization: Bearer <token>` on every protected request.

---

## Platform

- **OS target:** Windows (development), cross-platform (Docker containers run Linux images)
- **Runtime:** .NET 10 (backend), Node.js 18+ (frontend dev server)
- **Container orchestration:** .NET Aspire (local dev), Docker Desktop (container host)

---

## API Endpoints — 19 Total

### Health & Diagnostics (2)
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/` | No | Ping — returns `{ message: "Backend Running" }` |
| GET | `/health` | No | Health check — status, version, timestamp, DB name |

### Authentication (3)
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/auth/register` | No | Create a new user account |
| POST | `/auth/login` | No | Validate credentials, return JWT token |
| POST | `/auth/change-password` | JWT | Change the authenticated user's password |

### Users (5)
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/users` | JWT | List all active users |
| GET | `/users/{id}` | JWT | Get user profile by ID |
| GET | `/users/me` | JWT | Get the currently authenticated user's profile |
| PUT | `/users/{id}` | JWT | Update email / full name |
| DELETE | `/users/{id}` | JWT | Soft-deactivate a user (sets IsActive = false) |

### Products (7)
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/products` | No | List all products |
| GET | `/products/{id}` | No | Get product by ID |
| GET | `/products/search?q=` | No | Case-insensitive name search |
| GET | `/products/stats` | JWT | Stats: count, avg/min/max price |
| POST | `/products` | JWT | Create a new product |
| PUT | `/products/{id}` | JWT | Update a product |
| DELETE | `/products/{id}` | JWT | Delete a product |

### Dashboard (1)
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/dashboard/stats` | JWT | Total users, active users, total products |

### Test / Legacy (2)
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/hello` | No | Demo greeting |
| GET | `/secure` | JWT | Protected test endpoint |

---

## Security Measures

| Measure | Implementation |
|---------|---------------|
| Authentication | JWT Bearer tokens (HS256, 60-min expiry) |
| Password storage | BCrypt hashing (salted, per-user) |
| Rate limiting | 30 requests/minute per IP (ASP.NET Core RateLimiter) |
| CORS | Allow any `localhost` / `127.0.0.1` origin (dev-only policy) |
| Security headers | `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`, `Permissions-Policy`, `Strict-Transport-Security` |
| Input validation | Required field checks, email uniqueness, max-length constraints, minimum password length |
| Request logging | Custom middleware logs method + path for every request |
| Request timing | Custom middleware records response time |
| Global error handler | Returns structured JSON `{ error, status }` — no stack traces exposed |

---

## Database Schema

**Users**
```
Id           SERIAL PRIMARY KEY
Username     VARCHAR(100) NOT NULL UNIQUE
Email        VARCHAR(255) NOT NULL UNIQUE
PasswordHash TEXT NOT NULL
FullName     TEXT NOT NULL
CreatedAt    TIMESTAMPTZ
UpdatedAt    TIMESTAMPTZ
IsActive     BOOLEAN
```

**Products**
```
Id    SERIAL PRIMARY KEY
Name  VARCHAR(255) NOT NULL
Price DECIMAL NOT NULL
```

Migrations are applied automatically on API startup (`db.Database.Migrate()`). Seed data (1 user, 4 products) is inserted if the tables are empty.
