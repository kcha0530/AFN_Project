# Troubleshooting Guide

**Last Updated:** May 25, 2026

---

## Common Issues & Solutions

### 1. Aspire Won't Start

#### Error: "aspire: command not found"

**Cause:** Aspire CLI not installed with .NET 10

**Solution:**
```bash
# Install Aspire workload
dotnet workload install aspire

# Verify installation
dotnet workload list | grep aspire

# Update if needed
dotnet workload update
```

---

#### Error: "Port 18024 is already in use"

**Cause:** Aspire dashboard port conflict

**Solution:**
```bash
# Find process using port 18024
netstat -ano | findstr :18024  # Windows
lsof -i :18024                 # macOS/Linux

# Kill process
taskkill /PID <process-id> /F  # Windows
kill -9 <process-id>           # macOS/Linux

# OR use different port
aspire run --dashboard-port 18025
```

---

#### Error: "Docker daemon is not running"

**Cause:** Docker Desktop not started

**Solution:**
```bash
# Windows
# Open Docker Desktop application

# macOS
open /Applications/Docker.app

# Linux
sudo systemctl start docker

# Verify
docker ps
```

---

### 2. Database Connection Issues

#### Error: "FATAL: Ident authentication failed for user 'postgres'"

**Cause:** PostgreSQL authentication misconfiguration

**Solution:**
```bash
# Check container is running
docker ps | grep postgres

# View logs
docker logs <postgres-container-id>

# Restart container via Aspire
# Stop: Ctrl+C
# Start: aspire run
```

---

#### Error: "ERRORCODE=0x534 (1332), LEVEL=16, STATE=1: 'krit' is not a valid login"

**Cause:** First run - database seeding hasn't completed

**Solution:**
```bash
# Wait 30 seconds for migrations to complete
# Check Aspire dashboard for "apiservice" health status
# Should show "Healthy" green

# If still failing, check logs:
# Aspire Dashboard → apiservice → Logs tab

# Manual check with curl:
curl http://localhost:5474/health
# Should return 200 OK with health info
```

---

#### Error: "could not translate host name 'postgres' to address"

**Cause:** Service discovery not working in Aspire

**Solution:**
```bash
# This is normal during startup - Aspire is configuring services

# Wait for all services to show "Healthy" in dashboard

# Check connection string:
# Aspire Dashboard → apiservice → Environment variables

# Should show: Server=postgres;...
# NOT localhost:5432
```

---

### 3. Frontend Issues

#### Error: "Frontend app failed to start - ENOENT: no such file or directory, open 'package.json'"

**Cause:** npm dependencies not installed

**Solution:**
```bash
cd my-app
npm install
cd ..
aspire run
```

---

#### Error: "Vite port 5173 already in use"

**Cause:** Another process using the port

**Solution:**
```bash
# Method 1: Kill process using port
netstat -ano | findstr :5173
taskkill /PID <process-id> /F

# Method 2: Use different port
# Edit my-app/vite.config.js:
export default {
  server: {
    port: 5174
  }
}

# Method 3: Stop previous Aspire session
# Check: ps aux | grep "node\|aspire\|vite"
# Kill any stray processes
```

---

#### Error: "VITE_API_BASE_URL is undefined in frontend"

**Cause:** Environment variable not set in Aspire

**Solution:**
```bash
# Check my-app/.env.local exists:
cat my-app/.env.local
# Should contain: VITE_API_BASE_URL=http://localhost:5474

# If missing:
echo "VITE_API_BASE_URL=http://localhost:5474" > my-app/.env.local

# Restart Aspire
# Ctrl+C and: aspire run
```

---

#### Error: "API call returns 403 Forbidden"

**Cause:** CORS policy blocking request

**Solution:**
```bash
# Check browser console for CORS error details

# Verify frontend origin:
# Aspire Dashboard → frontend → Environment Variables
# Check if origin matches CORS policy in backend

# Approved origins in backend:
# - http://localhost:5173
# - http://127.0.0.1:5173

# If using different port or domain, update:
# backenddemo.ApiService/Program.cs:
options.AddPolicy("AllowFrontend", policy =>
{
    policy.WithOrigins("http://localhost:5174", ...)
        .AllowAnyHeader()
        .AllowAnyMethod();
});
```

---

### 4. Authentication Issues

#### Error: "401 Unauthorized" when calling protected endpoint

**Cause:** Missing or invalid JWT token

**Solution:**
```bash
# Check token is in localStorage:
# Browser DevTools → Application → Local Storage
# Should have: authToken, username, userId, email

# If missing, log in again:
# 1. Go to http://localhost:5173
# 2. Enter: username "krit", password "krit"
# 3. Click Sign In

# If still failing, check token expiration:
# JWT tokens valid for 60 minutes
# If older, log out and log in again
```

---

#### Error: "Password incorrect" but credentials are "krit/krit"

**Cause:** User not seeded properly in database

**Solution:**
```bash
# Reset database:
# 1. Stop Aspire: Ctrl+C
# 2. Find PostgreSQL container volume:
docker volume ls | grep postgres

# 3. Remove volume to reset data:
docker volume rm <volume-name>

# 4. Start Aspire:
aspire run

# 5. Wait 30 seconds for seeding

# 6. Try login with: krit / krit
```

---

#### Error: "Registration returns 'User already exists' but user doesn't show up"

**Cause:** Transaction rolled back, partial data

**Solution:**
```bash
# Check database state:
# Aspire Dashboard → PgAdmin link
# Navigate to: demodb → Tables → Users

# If orphaned record exists, delete it:
DELETE FROM "Users" WHERE "Username" = 'problematic_user';

# OR reset entire database (above solution)
```

---

### 5. API Endpoint Issues

#### Error: "POST /auth/login returns 400 Bad Request"

**Cause:** Invalid JSON or missing fields

**Solution:**
```bash
# Verify request format:
curl -X POST http://localhost:5474/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "krit", "password": "krit"}'

# Expected response:
# {"token": "eyJh...", "username": "krit", "userId": 1, "email": "krit@demo.com"}

# If error, check JSON syntax:
# ✅ Valid: {"username": "krit", "password": "krit"}
# ❌ Invalid: {'username': 'krit'}  (single quotes, no double quotes)
```

---

#### Error: "GET /products returns 500 Internal Server Error"

**Cause:** Database query failure

**Solution:**
```bash
# Check logs:
# Aspire Dashboard → apiservice → Logs tab

# Look for error messages like:
# - "Connection timeout"
# - "Table 'Products' does not exist"
# - "Deadlock detected"

# If "Table doesn't exist", run migrations:
cd backenddemo.ApiService
dotnet ef database update

# If connection timeout, restart database:
# Stop Aspire, wait 10 seconds, start again
```

---

#### Error: "429 Too Many Requests repeatedly"

**Cause:** Rate limiting (30 req/min per IP)

**Solution:**
```bash
# Slow down your requests:
# Instead of: for i in 1..100; do curl ...; done
# Do this:   for i in 1..100; do curl ...; sleep 0.5; done

# Check if multiple sessions are hitting same IP:
# From same machine: Only one session per IP
# From different machines: Each gets own quota

# Temporarily disable rate limit for testing:
# In Program.cs, comment out:
// app.UseRateLimiter();

# Wait 1 minute and try again (limit window passes)
```

---

### 6. Data Issues

#### Error: "Products table empty even after login"

**Cause:** Seeding code didn't run

**Solution:**
```bash
# Check seeding code ran:
# Aspire Dashboard → apiservice → Logs
# Look for: "Added X products to database"

# If not found, manually seed:
cd backenddemo.ApiService

# Create a migration-like script or:
# Use database UI (PgAdmin) to insert:
INSERT INTO "Products" ("Id", "Name", "Price") VALUES
  (1, 'Keyboard', 50),
  (2, 'Mouse', 30),
  (3, 'Monitor', 299),
  (4, 'USB Hub', 45);

# Then verify:
curl http://localhost:5474/products
```

---

#### Error: "User was deleted but still shows in list"

**Cause:** IsActive flag not checked or stale cache

**Solution:**
```bash
# Check IsActive flag:
UPDATE "Users" SET "IsActive" = false WHERE "Id" = <user-id>;

# Clear browser cache:
# DevTools → Application → Storage → Clear All

# Refresh page: F5 or Ctrl+R

# Or verify via API:
curl -H "Authorization: Bearer <token>" \
  http://localhost:5474/users

# Should only show IsActive = true users
```

---

### 7. Performance Issues

#### Problem: "API responses are slow (>1000ms)"

**Cause:** Multiple possibilities

**Solution:**
```bash
# Check database performance:
# Aspire Dashboard → apiservice → Logs
# Look for [Timing] entries: shows request duration

# If database slow (> 500ms):
# 1. Check PostgreSQL resource usage:
docker stats <postgres-container-id>

# 2. Increase container resources in AppHost.cs:
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithEnvironment("POSTGRES_INITDB_ARGS", "-c shared_buffers=256MB");

# 3. Restart Aspire

# If API slow but database fast:
# 1. Check for expensive operations
# 2. Profile code: dotnet trace collect
# 3. Check Redis caching is working
```

---

#### Problem: "Memory usage keeps increasing (memory leak)"

**Cause:** Objects not being garbage collected

**Solution:**
```bash
# Monitor memory in Aspire Dashboard
# Metrics tab → Memory (GB)

# If consistently increasing:
# 1. Check for circular references or retained objects
# 2. View GC stats:
dotnet counters monitor -n backenddemo.ApiService

# 3. Generate memory dump:
dotnet dump collect -n backenddemo.ApiService -o dump.dmp

# 4. Analyze with Visual Studio or dotMemory

# Immediate workaround:
# Restart Aspire periodically
aspire run  # runs until Ctrl+C
```

---

### 8. Aspire Dashboard Issues

#### Error: "Aspire Dashboard not loading or showing resources as 'Unknown'"

**Cause:** Dashboard service misconfiguration

**Solution:**
```bash
# Verify environment variables:
# Check terminal output for:
# ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL
# ASPIRE_RESOURCE_SERVICE_ENDPOINT_URL

# If using WSL or Docker:
# Use: host.docker.internal instead of localhost

# Access dashboard from different machine:
# Replace localhost with IP address in AppHost.cs:
var frontend = builder.AddNpmApp("frontend", ...)
    .WithEnvironment("VITE_API_BASE_URL", "http://<machine-ip>:5474");

# Firewall configuration:
# Allow port 18024, 5173, 5474
```

---

### 9. Code Changes Not Taking Effect

#### Problem: "Code changes not reflecting, stale response"

**Cause:** Hot reload not working or cache issue

**Solution:**
```bash
# For frontend (Vite hot reload):
# 1. Save file → should auto-refresh in browser
# 2. If not, check browser console for errors
# 3. Hard refresh: Ctrl+Shift+R (or Cmd+Shift+R)
# 4. Clear cache: DevTools → Application → Clear Site Data

# For backend (.NET hot reload):
# 1. Changes automatically recompile
# 2. Check Aspire dashboard for "Restarting apiservice..."
# 3. If not restarting, manually restart:
#    Ctrl+C, then: aspire run

# Force complete rebuild:
dotnet clean
dotnet build
aspire run
```

---

### 10. Docker Issues

#### Error: "Docker image not found for postgres:latest"

**Cause:** Docker image not available locally

**Solution:**
```bash
# Pull image manually:
docker pull postgres:latest

# Or let Aspire pull it:
# Just run: aspire run
# Aspire will pull needed images automatically

# Check what images are available:
docker images | grep postgres
```

---

#### Error: "No space left on device"

**Cause:** Docker volumes full

**Solution:**
```bash
# Check Docker disk usage:
docker system df

# Clean up unused data:
docker system prune -a --volumes

# WARNING: This deletes all stopped containers and unused images!
# Make sure database backups are created first

# After cleanup, restart:
aspire run
```

---

### 11. Getting Help

#### Before asking for help, gather this information:

```bash
# 1. Environment info
dotnet --version
docker --version
npm --version

# 2. Recent logs from Aspire Dashboard
# Take screenshot of error messages and logs

# 3. Your system
# Windows / macOS / Linux
# WSL version (if using WSL)

# 4. Exact error message
# Copy full error text from terminal/browser console

# 5. Steps to reproduce
# What were you doing when the error occurred?

# 6. What you've already tried
# List any troubleshooting steps taken
```

#### Useful diagnostic commands:

```bash
# Full diagnostic report:
dotnet diagnostics list
dotnet diagnostics ps

# Check .NET runtime:
dotnet --info

# Verify Aspire installation:
dotnet workload list

# Test connectivity:
curl -v http://localhost:5474/health
curl -v http://localhost:5173

# Check port availability:
netstat -ano | findstr ":5173\|:5474\|:18024"  # Windows
lsof -i :5173 -i :5474 -i :18024              # macOS/Linux
```

---

### 12. Emergency Procedures

#### "Everything is broken, need to reset"

```bash
# Complete reset procedure:

# 1. Stop everything
Ctrl+C  # Stop Aspire

# 2. Remove Docker containers and volumes
docker ps -a                          # List all
docker stop $(docker ps -aq)         # Stop all
docker rm $(docker ps -aq)           # Remove all
docker volume prune -f               # Remove unused volumes

# 3. Clear npm cache
cd my-app
npm cache clean --force
rm -rf node_modules package-lock.json
npm install
cd ..

# 4. Rebuild .NET projects
dotnet clean
dotnet build

# 5. Reinstall Aspire workload
dotnet workload uninstall aspire
dotnet workload install aspire

# 6. Start fresh
aspire run

# 7. Verify by accessing:
# - http://localhost:5173 (frontend)
# - http://localhost:5474/health (backend)
# - http://localhost:18024 (dashboard)
```

---

## Still Having Issues?

### Check These Documentation Files:
- **README.md** - Setup instructions
- **PROJECT_SUMMARY.md** - Architecture overview
- **API_DOCUMENTATION.md** - API endpoint details
- **SECURITY_GUIDE.md** - Security configuration

### Online Resources:
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [React Documentation](https://react.dev)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)

---

**Document Version:** 1.0.0  
**Last Updated:** May 25, 2026
