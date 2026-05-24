# AFN Project — Complete Run Guide

## Table of Contents
1. [Prerequisites](#1-prerequisites)
2. [Run the Full Stack (Recommended — via Aspire)](#2-run-the-full-stack-recommended)
3. [Run Frontend & Backend Separately (Manual)](#3-run-separately-manual)
4. [Access the Application](#4-access-the-application)
5. [Login Credentials](#5-login-credentials)
6. [Test APIs with Postman](#6-test-apis-with-postman)
7. [Manage PostgreSQL (PgAdmin & psql)](#7-manage-postgresql)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. Prerequisites

Install all of the following before running anything:

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 10.0+ | https://dot.net/download |
| Node.js | 18+ | https://nodejs.org |
| Docker Desktop | Latest | https://docker.com |
| Git | Any | https://git-scm.com |

**Check your versions:**
```powershell
dotnet --version       # Should be 10.x.x
node --version         # Should be v18+ or v20+
docker --version       # Should be running
```

> Docker Desktop **must be running** before you start. PostgreSQL and Redis run as containers managed by .NET Aspire.

---

## 2. Run the Full Stack (Recommended)

This is the easiest way. Aspire starts everything automatically:
PostgreSQL → Redis → API → React frontend.

```powershell
# 1. Open a terminal in the project root
cd D:\Krit\VSdef2

# 2. Install frontend npm packages (first time only)
cd frontend
npm install
cd ..

# 3. Start everything via Aspire AppHost
cd backenddemo.AppHost
dotnet run
```

You will see output like:
```
Login to the dashboard at: https://localhost:17xxxxx
```

Aspire will open the **dashboard** in your browser automatically.

> **First run takes 1-2 minutes** — Docker pulls PostgreSQL and Redis images.  
> Subsequent runs start in ~10 seconds.

---

## 3. Run Separately (Manual)

Use this if you want to run the frontend or backend independently.

### 3a. Backend only (API)

You need PostgreSQL running first. Either:
- Start via Aspire (step 2 above), or
- Run PostgreSQL manually:

```powershell
docker run -d --name pg-demo `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=demodb `
  -p 5432:5432 `
  postgres:15
```

Then run the API:
```powershell
cd D:\Krit\VSdef2\backenddemo.ApiService
dotnet run
```

API starts at `http://localhost:5474`  
Swagger UI at `http://localhost:5474/swagger`

### 3b. Frontend only

```powershell
cd D:\Krit\VSdef2\frontend
npm install        # first time only
npm run dev
```

Frontend starts at `http://localhost:5173`

> Set the API URL by creating `frontend/.env.local`:
> ```
> VITE_API_BASE_URL=http://localhost:5474
> ```

---

## 4. Access the Application

Once running, these URLs are available:

| Service | URL | Description |
|---------|-----|-------------|
| **React Frontend** | http://localhost:5173 | Main web app |
| **API (Swagger)** | http://localhost:5474/swagger | Interactive API docs |
| **Aspire Dashboard** | http://localhost:18024 | Logs, traces, metrics |
| **PgAdmin** | http://localhost:5050 | PostgreSQL GUI |
| **PostgreSQL** | localhost:5432 | Direct database access |

> Ports may vary slightly when run via Aspire. Check the Aspire dashboard for exact URLs.

---

## 5. Login Credentials

### Web App Login
```
Username: krit
Password: krit
```

Go to `http://localhost:5173`, enter the credentials above and click **Sign in**.  
You will be redirected to the protected dashboard automatically.

### PgAdmin Login (database UI)
```
Email:    admin@admin.com
Password: admin
```

---

## 6. Test APIs with Postman

### Step 1 — Import or create a collection

Open Postman → **New Collection** → name it `AFN Project API`

### Step 2 — Set a base URL variable

In the collection, click **Variables** and add:
```
Variable: baseUrl
Value:    http://localhost:5474
```

---

### 6a. Health Check

| Field | Value |
|-------|-------|
| Method | GET |
| URL | `{{baseUrl}}/` |

Expected response `200 OK`:
```json
{ "message": "Backend Running" }
```

---

### 6b. Login and get a JWT token

| Field | Value |
|-------|-------|
| Method | POST |
| URL | `{{baseUrl}}/auth/login` |
| Body (raw JSON) | see below |

```json
{
  "username": "krit",
  "password": "krit"
}
```

Response `200 OK`:
```json
{
  "token": "eyJhbGci...",
  "username": "krit",
  "userId": 1,
  "email": "krit@demo.com"
}
```

**Save the token:** Copy the `token` value. In Postman, go to your collection → **Authorization** tab → set type to **Bearer Token** and paste it. All authenticated requests will use it automatically.

---

### 6c. Product Endpoints

#### GET all products (public)
```
GET {{baseUrl}}/products
```

#### GET one product
```
GET {{baseUrl}}/products/1
```

#### Search products (public)
```
GET {{baseUrl}}/products/search?q=keyboard
```

#### GET product stats (requires token)
```
GET {{baseUrl}}/products/stats
Authorization: Bearer <token>
```

#### POST — Create product (requires token)
```
POST {{baseUrl}}/products
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 0,
  "name": "Wireless Headset",
  "price": 89.99
}
```

#### PUT — Update product (requires token)
```
PUT {{baseUrl}}/products/1
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "name": "Mechanical Keyboard",
  "price": 75.00
}
```

#### DELETE product (requires token)
```
DELETE {{baseUrl}}/products/5
Authorization: Bearer <token>
```

---

### 6d. User Endpoints (all require token)

#### GET all users
```
GET {{baseUrl}}/users
Authorization: Bearer <token>
```

#### GET current user profile
```
GET {{baseUrl}}/users/me
Authorization: Bearer <token>
```

#### Dashboard stats
```
GET {{baseUrl}}/dashboard/stats
Authorization: Bearer <token>
```

---

### 6e. Register a new user

```
POST {{baseUrl}}/auth/register
Content-Type: application/json

{
  "username": "alice",
  "email": "alice@example.com",
  "password": "alice123",
  "fullName": "Alice Smith"
}
```

---

### 6f. Postman — Quick status code reference

| Code | Meaning |
|------|---------|
| 200 | OK — success |
| 201 | Created — new record |
| 400 | Bad Request — invalid input |
| 401 | Unauthorized — missing/invalid token |
| 404 | Not Found |
| 429 | Too Many Requests (rate limited) |
| 500 | Server Error |

---

## 7. Manage PostgreSQL

### Option A — Swagger (easiest, no setup)

Go to `http://localhost:5474/swagger` and use the interactive UI to call any endpoint.

---

### Option B — PgAdmin (GUI)

1. Open `http://localhost:5050`
2. Login: Email `admin@admin.com`, Password `admin`
3. Click **Add New Server**:
   - **Name:** `demodb`
   - **Host:** `postgres` (or `localhost` if running standalone)
   - **Port:** `5432`
   - **Database:** `demodb`
   - **Username:** `postgres`
   - **Password:** `postgres`
4. Click **Save**

You can now browse tables, run SQL queries, and view data.

**Useful queries to run in PgAdmin Query Tool:**

```sql
-- View all users
SELECT id, username, email, "fullName", "isActive", "createdAt"
FROM "Users";

-- View all products
SELECT * FROM "Products";

-- Count records
SELECT COUNT(*) as users FROM "Users";
SELECT COUNT(*) as products FROM "Products";

-- Find a specific user
SELECT * FROM "Users" WHERE username = 'krit';

-- Reset a user's password (BCrypt hash for 'newpassword123')
-- Use the app's /auth/change-password endpoint instead for safety
```

---

### Option C — psql CLI

```powershell
# Connect inside the Docker container
docker exec -it <container_name> psql -U postgres -d demodb

# Find the container name first:
docker ps
```

Inside psql:
```sql
\dt                          -- list tables
SELECT * FROM "Users";       -- view users
SELECT * FROM "Products";    -- view products
\q                           -- quit
```

---

### Understanding the Database Schema

#### Users table
```
Id          SERIAL PRIMARY KEY
Username    VARCHAR(100) UNIQUE NOT NULL
Email       VARCHAR(255) UNIQUE NOT NULL
PasswordHash TEXT NOT NULL (BCrypt hashed)
FullName    TEXT
CreatedAt   TIMESTAMPTZ
UpdatedAt   TIMESTAMPTZ
IsActive    BOOLEAN (soft delete flag)
```

#### Products table
```
Id    SERIAL PRIMARY KEY
Name  VARCHAR(255) NOT NULL
Price DECIMAL NOT NULL
```

---

## 8. Troubleshooting

### "Cannot connect to Docker daemon"
Start Docker Desktop and wait 30 seconds, then retry.

### "Port 5432 already in use"
Another PostgreSQL instance is running. Stop it:
```powershell
# Find what's using the port
netstat -ano | findstr :5432
# Kill by PID
taskkill /PID <pid> /F
```

### "Connection refused" from frontend to API
1. Ensure the API is running at `http://localhost:5474`
2. Check `frontend/.env.local` has `VITE_API_BASE_URL=http://localhost:5474`
3. Restart the Vite dev server after changing `.env.local`

### JWT "401 Unauthorized" in Postman
- Your token has expired (60-minute lifetime). Re-login and copy the new token.
- Ensure the `Authorization` header is `Bearer <token>` (with the word Bearer and a space).

### Database migration errors
```powershell
cd D:\Krit\VSdef2\backenddemo.ApiService
dotnet ef database drop --force
dotnet ef database update
```

### Frontend blank page after login
Open browser DevTools (F12) → Console. If you see CORS errors, ensure:
- API is running and CORS is set to allow `localhost`
- You are accessing the site via `http://localhost:5173` (not `127.0.0.1`)

### npm install fails
```powershell
cd D:\Krit\VSdef2\frontend
Remove-Item -Recurse -Force node_modules
npm cache clean --force
npm install
```
