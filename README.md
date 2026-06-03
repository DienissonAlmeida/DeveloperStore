# DeveloperStore API

.NET 10 REST API with layered architecture, Serilog, Swagger, and JWT authentication.

## Architecture

```
src/
├── DeveloperStore.Domain          # Entities and repository interfaces
├── DeveloperStore.Application     # Use cases, DTOs, application interfaces
├── DeveloperStore.Infrastructure  # Identity, persistence, external services
└── DeveloperStore.Api             # Controllers, middleware, Program.cs
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/products/docker-desktop) (optional)

---

## Running locally

```bash
# 1. Clone the repo
git clone https://github.com/DienissonAlmeida/DeveloperStore.git
cd DeveloperStore

# 2. Run the API
dotnet run --project src/DeveloperStore.Api
```

Swagger UI will be available at: http://localhost:5000/swagger

---

## Running with Docker

### docker compose (recommended)

```bash
docker compose up --build
```

API available at: http://localhost:8080/swagger

### docker build + run (manual)

```bash
# Build the image
docker build -t developerstore-api .

# Run the container
docker run -p 8080:8080 \
  -e Jwt__Key="CHANGE_ME_TO_A_STRONG_SECRET_KEY_AT_LEAST_32_CHARS" \
  -e Jwt__Issuer="DeveloperStore" \
  -e Jwt__Audience="DeveloperStore" \
  developerstore-api
```

---

## Authentication

The API uses JWT Bearer tokens. To get a token:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@developerstore.com",
  "password": "Admin@123"
}
```

Use the returned token in the `Authorization` header:

```
Authorization: Bearer <token>
```

Or paste it directly into the Swagger UI **Authorize** button.

---

## Configuration

Key settings live in `appsettings.json`. Override via environment variables using double-underscore as separator:

| Setting | Env var | Default |
|---|---|---|
| JWT signing key | `Jwt__Key` | *(change this)* |
| JWT issuer | `Jwt__Issuer` | `DeveloperStore` |
| JWT audience | `Jwt__Audience` | `DeveloperStore` |
| Token expiry (minutes) | `Jwt__ExpiresInMinutes` | `60` |

> **Never commit a real secret to source control.** Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local dev and environment variables or a secrets manager in production.

---

## Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | No | Get a JWT token |
| `GET` | `/api/health` | No | Health check |
| `GET` | `/api/health/secure` | Yes | Authenticated health check |
