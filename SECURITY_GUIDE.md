# Security Guide

**Last Updated:** May 25, 2026

---

## 🔐 Security Overview

This document outlines all security measures implemented in the backenddemo application and best practices for secure deployment.

---

## 1. Authentication & Authorization

### 1.1 JWT (JSON Web Tokens)

**Implementation:**
```
Algorithm: HS256 (HMAC SHA-256)
Signing: Symmetric key (shared between issuer and validator)
Expiration: 60 minutes
```

**Token Structure:**
```json
{
  "sub": "username",           // Subject (username)
  "UserId": "1",              // Custom claim: User ID
  "jti": "unique-id",         // JWT ID (prevents replay)
  "exp": 1234567890,          // Expiration timestamp
  "iss": "backenddemo.ApiService",    // Issuer
  "aud": "backenddemo.Web"            // Audience
}
```

**Security Properties:**
✅ Signature validated on each request  
✅ Expiration checked  
✅ Issuer/Audience mismatch rejected  
✅ Claims extracted and available to handlers  

**Potential Improvements:**
- [ ] Use RS256 asymmetric signing in production
- [ ] Implement refresh token rotation
- [ ] Add token blacklist for logout
- [ ] Store token metadata in Redis

### 1.2 Password Security

**Implementation:**
```
Hashing Algorithm: BCrypt
Rounds: 10 (default)
Salt: Automatically generated per password
```

**Code Example:**
```csharp
// Hashing
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

// Verification
bool isValid = BCrypt.Net.BCrypt.Verify(password, storedHash);
```

**Security Properties:**
✅ Passwords never stored plaintext  
✅ One-way hashing (cannot be reversed)  
✅ Salt prevents rainbow table attacks  
✅ Time-constant comparison prevents timing attacks  

**Auditing:**
```sql
-- Check all users have hashes
SELECT COUNT(*) FROM Users WHERE PasswordHash IS NULL OR PasswordHash = '';
-- Result should be 0
```

### 1.3 Protected Routes

Routes requiring `[Authorize]` attribute:

| Endpoint | Method | Protection |
|----------|--------|-----------|
| /users | GET | JWT Required |
| /users/{id} | GET, PUT | JWT Required |
| /products | POST, PUT, DELETE | JWT Required |
| /dashboard/stats | GET | JWT Required |
| /secure | GET | JWT Required |

---

## 2. Network Security

### 2.1 HTTPS/TLS

**In Development:**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();                  // Strict-Transport-Security
    app.UseHttpsRedirection();      // Redirect HTTP to HTTPS
}
```

**In Production:**
✅ Enable HSTS  
✅ Redirect HTTP → HTTPS  
✅ Use valid SSL/TLS certificate  
✅ Set HSTS preload flag  

**Certificate Management:**
```bash
# Using Let's Encrypt (free)
dotnet dev-certs https --trust

# Using Azure Key Vault (production)
az keyvault certificate create --vault-name myKeyVault ...

# Using self-signed (testing only)
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
```

### 2.2 CORS (Cross-Origin Resource Sharing)

**Configured Origins:**
```csharp
options.AddPolicy("AllowFrontend", policy =>
{
    policy
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod();
});
```

**Security Implications:**
✅ Only specified origins can access API  
✅ Prevents unauthorized frontend domains  
✅ Credentials not automatically sent (safe)  

**Production Configuration:**
```csharp
// Use environment variables
var allowedOrigins = configuration["AllowedCorsOrigins"]
    .Split(',')
    .ToArray();

policy.WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();  // If using cookies
```

### 2.3 HTTP Security Headers

**Implemented Headers:**

```csharp
context.Response.Headers["X-Content-Type-Options"] = "nosniff";
// ├─ Prevents MIME-type sniffing attacks
// └─ Browser must trust Content-Type header

context.Response.Headers["X-Frame-Options"] = "DENY";
// ├─ Prevents clickjacking
// └─ Page cannot be framed in any context

context.Response.Headers["Referrer-Policy"] = "no-referrer";
// ├─ Hides referrer information
// └─ Prevents leaking of referer URLs

context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";
// ├─ Disables unnecessary browser APIs
// └─ Reduces attack surface

context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
// ├─ Forces HTTPS for 1 year
// ├─ Includes subdomains
// └─ Preload eligible
```

**Additional Headers (Recommended):**
```csharp
context.Response.Headers["Content-Security-Policy"] = 
    "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";

context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
```

---

## 3. Rate Limiting

### 3.1 Configuration

**Current Policy:**
```
Permit Limit: 30 requests
Window: 1 minute
Partition: Client IP address
Queue Limit: 0 (reject when limit exceeded)
```

**Code:**
```csharp
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
{
    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    return RateLimitPartition.GetFixedWindowLimiter(clientIp, 
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
});
```

### 3.2 Response When Limited

```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json

{
  "error": "Too many requests. Please try again later."
}
```

### 3.3 Bypass Strategy

Some endpoints should bypass rate limiting:
```csharp
app.MapGet("/health", ...)
    .WithName("health")
    .WithOpenApi()
    .DisableRateLimiting();  // Add this
```

---

## 4. Input Validation

### 4.1 Server-Side Validation

**Current Implementations:**

```csharp
// Login endpoint
if (login is null || 
    string.IsNullOrWhiteSpace(login.Username) || 
    string.IsNullOrWhiteSpace(login.Password))
{
    return Results.BadRequest(...);
}

// Registration endpoint
if (string.IsNullOrWhiteSpace(request.Username) || 
    string.IsNullOrWhiteSpace(request.Password) || 
    string.IsNullOrWhiteSpace(request.Email))
{
    return Results.BadRequest(...);
}

// Duplicate check
var existingUser = await db.Users.FirstOrDefaultAsync(
    u => u.Username == request.Username || u.Email == request.Email
);
if (existingUser != null)
{
    return Results.BadRequest(new { error = "Already exists" });
}
```

### 4.2 Database-Level Constraints

```sql
-- Unique constraints prevent duplicates
CONSTRAINT IX_Users_Username UNIQUE (Username)
CONSTRAINT IX_Users_Email UNIQUE (Email)

-- Max length enforced
Username VARCHAR(100) NOT NULL
Email VARCHAR(255) NOT NULL
```

### 4.3 SQL Injection Prevention

**Protected by:**
```csharp
// EF Core uses parameterized queries
var user = await db.Users
    .FirstOrDefaultAsync(u => u.Username == login.Username);
    // ├─ Parameter binding
    // ├─ Query compilation
    // └─ Type checking

// NOT vulnerable:
string query = $"SELECT * FROM Users WHERE Username = '{input}'";  // ❌ NEVER DO THIS
```

---

## 5. Data Protection

### 5.1 Sensitive Data Classification

| Data | Classification | Storage | Encryption |
|------|---|---|---|
| Username | Public | Database | No |
| Email | Confidential | Database | No (but hashed for lookup) |
| Password | Secret | Database | ✅ BCrypt Hash |
| JWT Token | Sensitive | localStorage (frontend) | ✅ Signed |
| JWT Secret | Critical | appsettings.json (dev) | ⚠️ Use secrets manager (prod) |

### 5.2 Secret Management

**Development:**
```bash
# Using User Secrets
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-secret-key"
dotnet user-secrets set "ConnectionStrings:Default" "..."
```

**Production:**
```csharp
// Azure Key Vault
builder.Configuration.AddAzureKeyVault(
    vaultUri: new Uri("https://keyvault-name.vault.azure.net/"),
    new DefaultAzureCredential());

// AWS Secrets Manager
builder.Configuration.AddSecretsManager(region: RegionEndpoint.USEast1);

// Environment Variables
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
    ?? throw new InvalidOperationException("JWT_KEY not set");
```

### 5.3 Database Connection Security

```csharp
// Current (Development)
"ConnectionStrings": {
    "demodb": "Server=postgres;Database=demodb;Username=postgres;Password=postgres;"
}

// Production (Never hardcode!)
string connectionString = configuration.GetConnectionString("demodb")
    ?? throw new InvalidOperationException("Connection string not configured");

// With SSL requirement
"Server=db.example.com;Database=demodb;Username=dbuser;Password=***;SSL Mode=Require;Trust Server Certificate=false;"
```

---

## 6. Logging & Auditing

### 6.1 Implemented Logging

```csharp
// Request logging
Console.WriteLine($"[Request] {context.Request.Method} {context.Request.Path}");

// Timing logging
Console.WriteLine($"[Timing] {context.Request.Method} {context.Request.Path} " +
    $"completed in {stopwatch.ElapsedMilliseconds} ms");

// Error logging
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        // Log exception details
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        logger.LogError(exception, "Unhandled exception");
    });
});
```

### 6.2 Sensitive Data in Logs

**Never log:**
- ❌ Passwords (plaintext or hashed)
- ❌ JWT tokens (full token)
- ❌ Credit card numbers
- ❌ Social security numbers
- ❌ API keys

**Safe to log:**
- ✅ Username (masked if needed)
- ✅ User ID
- ✅ Request method & path
- ✅ Response status code
- ✅ Timestamp
- ✅ Client IP

### 6.3 Audit Trail Example

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; }          // "LOGIN", "PRODUCT_CREATE"
    public string EntityType { get; set; }      // "User", "Product"
    public int? EntityId { get; set; }
    public string OldValue { get; set; }        // JSON before change
    public string NewValue { get; set; }        // JSON after change
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}
```

---

## 7. API Security Best Practices

### 7.1 Input Size Limits

```csharp
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = 4096;         // 4 KB per field
    options.MultipartBodyLengthLimit = 268435456;  // 256 MB total
});
```

### 7.2 Output Encoding

**In React (automatic):**
```jsx
// Automatically escaped
<div>{userInput}</div>

// NOT escaped (dangerous)
<div dangerouslySetInnerHTML={{__html: userInput}} />  // ❌ NEVER USE
```

### 7.3 Error Messages

**Secure error responses:**
```csharp
// ✅ GOOD - Generic message
return Results.Unauthorized();

// ❌ BAD - Information disclosure
return Results.Unauthorized(new { 
    error = "Username 'user123' not found in database"
});

// ✅ GOOD - For non-sensitive endpoints
return Results.BadRequest(new { 
    error = "Invalid email format"
});
```

---

## 8. Database Security

### 8.1 Connection Pooling

```csharp
// EF Core default: 100 connections
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, dbOptions =>
    {
        dbOptions.MaxPoolSize = 50);  // Limit connections
    });
});
```

### 8.2 Backup & Recovery

```bash
# Backup PostgreSQL
pg_dump -U postgres -d demodb > backup.sql

# Restore
psql -U postgres -d demodb < backup.sql

# With encryption (GPG)
pg_dump demodb | gzip | gpg -e > backup.sql.gz.gpg
```

### 8.3 Access Control

```sql
-- Create limited user (not admin)
CREATE USER apiuser WITH PASSWORD '***';
GRANT CONNECT ON DATABASE demodb TO apiuser;
GRANT USAGE ON SCHEMA public TO apiuser;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO apiuser;

-- Revoke dangerous privileges
REVOKE SUPERUSER ON apiuser;
REVOKE CREATEDB ON apiuser;
```

---

## 9. Deployment Security Checklist

- [ ] **Environment Variables Set**
  - [ ] JWT_KEY - Production-grade secret
  - [ ] CONNECTION_STRING - Remote DB with SSL
  - [ ] LOG_LEVEL - Set to Warning or higher

- [ ] **HTTPS Enabled**
  - [ ] Valid SSL certificate installed
  - [ ] HSTS enabled (max-age ≥ 31536000)
  - [ ] HTTP redirect to HTTPS

- [ ] **Database Hardening**
  - [ ] PostgreSQL not exposed publicly
  - [ ] Firewall rules restrict access
  - [ ] Encrypted connections (SSL required)
  - [ ] Regular backups enabled

- [ ] **API Hardening**
  - [ ] Rate limiting enabled
  - [ ] Security headers configured
  - [ ] CORS origins restricted
  - [ ] Input validation on all endpoints

- [ ] **Monitoring & Logging**
  - [ ] Centralized logging enabled (e.g., ELK, Splunk)
  - [ ] Error alerts configured
  - [ ] Performance monitoring active
  - [ ] Security audit logging enabled

- [ ] **Secrets Management**
  - [ ] No secrets in source code
  - [ ] Azure Key Vault / AWS Secrets Manager configured
  - [ ] Rotation policy enforced
  - [ ] Access logs reviewed

---

## 10. Incident Response

### 10.1 Security Breach Response

**If JWT secret is compromised:**
1. Immediately rotate the secret
2. Force re-login for all users
3. Invalidate all tokens (if token blacklist implemented)
4. Review audit logs for unauthorized access
5. Notify affected users

**If database is compromised:**
1. Take database offline
2. Restore from latest backup
3. Change all admin credentials
4. Audit all changes since last backup
5. Investigate root cause
6. Implement additional monitoring

### 10.2 Vulnerability Scan

```bash
# Check for vulnerabilities in dependencies
dotnet list package --vulnerable

# Audit npm packages
npm audit

# OWASP Dependency Check
dotnet tool install --global DependencyCheck
dependency-check --project "backenddemo" --scan .
```

---

## 11. Compliance & Standards

### 11.1 Standards Compliance

- ✅ **OWASP Top 10** - Mitigated common vulnerabilities
- ✅ **NIST Cybersecurity Framework** - Following best practices
- ✅ **CWE/SANS Top 25** - Addressed critical issues
- ✅ **RFC 7519 (JWT)** - Compliant token implementation
- ✅ **RFC 6234 (SHA)** - Secure hashing algorithms

### 11.2 Data Protection Regulations

**GDPR Compliance (if applicable):**
- [ ] User consent for data collection
- [ ] Privacy policy published
- [ ] Data deletion mechanism implemented
- [ ] Data export functionality available
- [ ] Breach notification process documented

---

## 12. Security Testing

### 12.1 Manual Testing

```http
# Test SQL Injection
POST /auth/login HTTP/1.1
Content-Type: application/json

{"username": "' OR '1'='1", "password": "anything"}
// Expected: 401 Unauthorized

# Test Rate Limiting
for i in {1..35}; do curl http://localhost:5474/health; done
// Expected: First 30 succeed, then 429 Too Many Requests

# Test Missing Auth
GET /users HTTP/1.1
// Expected: 401 Unauthorized

# Test CORS Preflight
OPTIONS /users HTTP/1.1
Origin: http://attacker.com
// Expected: 403 Forbidden or CORS headers not present
```

### 12.2 Automated Testing

```bash
# OWASP ZAP scanning
docker run -t owasp/zap2docker-stable -t http://localhost:5474

# NMAP vulnerability scanning
nmap -sV -p 5474 localhost

# Burp Suite Community (GUI tool)
burpsuite &
```

---

## 13. Security Update Policy

**Update Cadence:**
- Critical: Within 24 hours
- High: Within 1 week
- Medium: Within 2 weeks
- Low: Within 1 month

**Monitoring:**
```bash
# Check for updates monthly
dotnet outdated

# NuGet security advisories
https://security.nuget.org/

# npm security advisories
npm audit
https://github.com/advisories
```

---

## 14. Contact & Reporting

**Security Issues:**
- ❌ Do NOT file GitHub issues for security vulnerabilities
- ✅ Email: security@example.com
- ✅ PGP Key: [Add if applicable]
- ✅ Response Time: 48 hours
- ✅ CVE Eligibility: Will be requested if appropriate

---

**Document Version:** 1.0.0  
**Last Reviewed:** May 25, 2026  
**Next Review:** June 25, 2026
