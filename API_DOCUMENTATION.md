# API Documentation

**Base URL:** `http://localhost:5474`  
**API Version:** 1.0.0  
**Authentication:** JWT Bearer Token

---

## 📋 Table of Contents
1. [Health & Info](#health--info)
2. [Authentication](#authentication)
3. [User Management](#user-management)
4. [Products](#products)
5. [Dashboard](#dashboard)
6. [Error Responses](#error-responses)
7. [Authentication Header](#authentication-header)

---

## Health & Info

### GET /
**Description:** Basic health check  
**Authentication:** Not required  
**Rate Limited:** Yes

```http
GET http://localhost:5474/
```

**Response (200 OK):**
```json
{
  "message": "Backend Running"
}
```

---

### GET /health
**Description:** Detailed health check with database stats  
**Authentication:** Not required  
**Rate Limited:** Yes

```http
GET http://localhost:5474/health
```

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "version": "1.0.0",
  "timestamp": "2026-05-25T10:30:45.123456Z",
  "database": "PostgreSQL"
}
```

---

## Authentication

### POST /auth/register
**Description:** Register a new user  
**Authentication:** Not required  
**Rate Limited:** Yes

```http
POST http://localhost:5474/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "SecurePass123!",
  "fullName": "New User"
}
```

**Request Parameters:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| username | string | Yes | Unique username (max 100 chars) |
| email | string | Yes | Valid email (max 255 chars) |
| password | string | Yes | Password (plaintext, will be hashed) |
| fullName | string | Yes | User's full name |

**Response (201 Created):**
```json
{
  "id": 2,
  "username": "newuser",
  "email": "newuser@example.com",
  "fullName": "New User",
  "createdAt": "2026-05-25T10:30:45.123456Z"
}
```

**Error Responses:**
- `400 Bad Request` - Missing required fields
  ```json
  {"error": "Username, email, and password are required."}
  ```
- `400 Bad Request` - User already exists
  ```json
  {"error": "Username or email already exists."}
  ```
- `429 Too Many Requests` - Rate limited
  ```json
  {"error": "Too many requests. Please try again later."}
  ```

---

### POST /auth/login
**Description:** Authenticate user and get JWT token  
**Authentication:** Not required  
**Rate Limited:** Yes

```http
POST http://localhost:5474/auth/login
Content-Type: application/json

{
  "username": "krit",
  "password": "krit"
}
```

**Request Parameters:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| username | string | Yes | User's username |
| password | string | Yes | User's password (plaintext) |

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "krit",
  "userId": 1,
  "email": "krit@demo.com"
}
```

**Response Fields:**
| Field | Type | Description |
|-------|------|-------------|
| token | string | JWT Bearer token (valid for 60 minutes) |
| username | string | Username of authenticated user |
| userId | integer | User ID for subsequent requests |
| email | string | User's email address |

**Error Responses:**
- `400 Bad Request` - Missing credentials
  ```json
  {"error": "Username and password are required."}
  ```
- `401 Unauthorized` - Invalid credentials
  ```json
  {}  // Generic response for security
  ```
- `401 Unauthorized` - Account inactive
  ```json
  {"error": "User account is inactive."}
  ```

---

## User Management

### GET /users
**Description:** List all active users  
**Authentication:** Required ✅  
**Rate Limited:** Yes

```http
GET http://localhost:5474/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "username": "krit",
    "email": "krit@demo.com",
    "fullName": "Krit Chaiyabud",
    "createdAt": "2026-05-25T10:30:45.123456Z"
  },
  {
    "id": 2,
    "username": "newuser",
    "email": "newuser@example.com",
    "fullName": "New User",
    "createdAt": "2026-05-25T10:31:15.234567Z"
  }
]
```

---

### GET /users/{id}
**Description:** Get specific user profile  
**Authentication:** Required ✅  
**Rate Limited:** Yes

```http
GET http://localhost:5474/users/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | integer | Yes | User ID |

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "krit",
  "email": "krit@demo.com",
  "fullName": "Krit Chaiyabud",
  "createdAt": "2026-05-25T10:30:45.123456Z"
}
```

**Error Responses:**
- `401 Unauthorized` - No or invalid token
- `404 Not Found` - User not found
  ```json
  {"error": "User not found"}
  ```

---

### PUT /users/{id}
**Description:** Update user profile  
**Authentication:** Required ✅  
**Rate Limited:** Yes

```http
PUT http://localhost:5474/users/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "email": "krit.new@demo.com",
  "fullName": "Krit Updated"
}
```

**Request Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| email | string | No | New email address |
| fullName | string | No | New full name |

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "krit",
  "email": "krit.new@demo.com",
  "fullName": "Krit Updated",
  "createdAt": "2026-05-25T10:30:45.123456Z"
}
```

**Error Responses:**
- `401 Unauthorized` - No or invalid token
- `404 Not Found` - User not found

---

## Products

### GET /products
**Description:** List all products  
**Authentication:** Not required  
**Rate Limited:** Yes

```http
GET http://localhost:5474/products
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Keyboard",
    "price": 50.00
  },
  {
    "id": 2,
    "name": "Mouse",
    "price": 30.00
  },
  {
    "id": 3,
    "name": "Monitor 27\"",
    "price": 299.00
  },
  {
    "id": 4,
    "name": "USB-C Hub",
    "price": 45.00
  }
]
```

---

### GET /products/{id}
**Description:** Get specific product details  
**Authentication:** Not required  
**Rate Limited:** Yes

```http
GET http://localhost:5474/products/1
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | integer | Yes | Product ID |

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Keyboard",
  "price": 50.00
}
```

**Error Responses:**
- `404 Not Found` - Product not found
  ```json
  {"message": "Product not found"}
  ```

---

### POST /products
**Description:** Create new product  
**Authentication:** Required ✅  
**Rate Limited:** Yes

```http
POST http://localhost:5474/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "id": 5,
  "name": "Laptop",
  "price": 1299.99
}
```

**Request Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | integer | No | Product ID (auto-generated if omitted) |
| name | string | Yes | Product name (max 255 chars) |
| price | decimal | Yes | Product price |

**Response (201 Created):**
```json
{
  "id": 5,
  "name": "Laptop",
  "price": 1299.99
}
```

**Error Responses:**
- `401 Unauthorized` - No or invalid token
- `400 Bad Request` - Invalid input

---

### PUT /products/{id}
**Description:** Update existing product  
**Authentication:** Required ✅  
**Rate Limited:** Yes

```http
PUT http://localhost:5474/products/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "id": 1,
  "name": "Mechanical Keyboard",
  "price": 75.00
}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | integer | Yes | Product ID to update |

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Mechanical Keyboard",
  "price": 75.00
}
```

**Error Responses:**
- `401 Unauthorized` - No or invalid token
- `404 Not Found` - Product not found

---

### DELETE /products/{id}
**Description:** Delete product  
**Authentication:** Required ✅  
**Rate Limited:** Yes

```http
DELETE http://localhost:5474/products/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | integer | Yes | Product ID to delete |

**Response (200 OK):**
```json
{
  "message": "Product deleted"
}
```

**Error Responses:**
- `401 Unauthorized` - No or invalid token
- `404 Not Found` - Product not found

---

## Dashboard

### GET /dashboard/stats
**Description:** Get dashboard statistics  
**Authentication:** Required ✅  
**Rate Limited:** Yes

```http
GET http://localhost:5474/dashboard/stats
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK):**
```json
{
  "totalUsers": 2,
  "totalProducts": 4,
  "activeUsers": 2,
  "lastUpdated": "2026-05-25T10:35:20.345678Z"
}
```

**Response Fields:**
| Field | Type | Description |
|-------|------|-------------|
| totalUsers | integer | Total user accounts |
| totalProducts | integer | Total products in catalog |
| activeUsers | integer | Users with IsActive = true |
| lastUpdated | datetime | Timestamp of stats generation |

**Error Responses:**
- `401 Unauthorized` - No or invalid token

---

## Error Responses

### Common Error Formats

**401 Unauthorized**
```json
{
  "error": "Unauthorized"
}
```

**404 Not Found**
```json
{
  "error": "Route not found",
  "status": 404
}
```

**429 Too Many Requests**
```json
{
  "error": "Too many requests. Please try again later."
}
```

**500 Internal Server Error**
```json
{
  "error": "Internal Server Error",
  "status": 500
}
```

---

## Authentication Header

### JWT Bearer Token Format

All protected endpoints require this header:

```http
Authorization: Bearer <JWT_TOKEN>
```

**Example:**
```http
GET /users HTTP/1.1
Host: localhost:5474
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJrcml0IiwiVXNlcklkIjoiMSIsImp0aSI6ImQ2NDI0ZjY1LTljODItNDQwYS04MzI5LWQ1ZmVhYTljYTAxZCIsImV4cCI6MTc0NjkzNTQwMSwiaXNzIjoiYmFja2VuZGRlbW8uQXBpU2VydmljZSIsImF1ZCI6ImJhY2tlbmRkZW1vLldlYiJ9.X5hK...
```

### JWT Token Structure

The JWT contains these claims:
```json
{
  "sub": "krit",                              // username (Subject)
  "UserId": "1",                              // user ID
  "jti": "d6424f65-9c82-440a-8329-...",     // JWT ID
  "exp": 1746935401,                         // expiration timestamp
  "iss": "backenddemo.ApiService",           // issuer
  "aud": "backenddemo.Web"                   // audience
}
```

**Token Validity:** 60 minutes from issue time

---

## Testing with Swagger

Interactive API testing available at:
```
http://localhost:5474/swagger
```

All endpoints listed with try-it-out functionality.

---

## Rate Limiting

**Policy:** 30 requests per minute per IP address

**Response when limited:**
- HTTP 429 Too Many Requests
- Retry-After header included
- JSON error response

---

## CORS Policy

**Allowed Origins:**
- `http://localhost:5173`
- `http://127.0.0.1:5173`

**Allowed Methods:** GET, POST, PUT, DELETE, OPTIONS  
**Allowed Headers:** All

---

## API Version

**Current Version:** 1.0.0  
**Last Updated:** May 25, 2026  
**Status:** Production Ready ✅
