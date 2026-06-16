# Secure Task API

A secure, multi-user task-management REST API built with **ASP.NET Core (.NET 10)**.
Users register and log in, receive a **JWT**, and manage their own private to-do
items. Every task is scoped to its owner, so one user can never see or modify
another user's data.

This project demonstrates a production-style backend: token-based authentication,
password hashing, a service layer, EF Core migrations, automated tests,
Swagger documentation, and a one-command Docker setup.

---

## Features

- **JWT authentication** – stateless bearer-token auth via `Microsoft.AspNetCore.Authentication.JwtBearer`.
- **Password security** – passwords are hashed with **BCrypt**; plain-text passwords are never stored.
- **Per-user data isolation** – the user id is read from the JWT claim, so the API only ever returns the caller's own tasks.
- **Full CRUD** for tasks (`GET` / `POST` / `PUT` / `DELETE`).
- **EF Core + SQL Server** with automatic database migration on startup.
- **Swagger / OpenAPI** UI with a built-in "Authorize" button for testing protected endpoints.
- **Global exception handling** – unhandled errors return a clean JSON message instead of a stack trace.
- **Unit tests** (xUnit + EF Core In-Memory) covering the task service, including the data-isolation rule.
- **Dockerised** – `docker compose up` starts the API and a SQL Server instance together.

## Tech stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core / .NET 10 |
| Auth | JWT Bearer tokens |
| Password hashing | BCrypt.Net-Next |
| Data access | Entity Framework Core 10 (SQL Server) |
| API docs | Swashbuckle (Swagger) |
| Tests | xUnit + EF Core In-Memory |
| Containerisation | Docker + Docker Compose |

## Architecture

```
Controllers  ->  Services (ITaskService)  ->  EF Core DbContext  ->  SQL Server
   Auth/JWT  ->  JwtTokenService
```

Controllers stay thin and delegate business logic to the service layer, which makes
the logic easy to unit-test without spinning up a web server or a real database.

---

## Getting started

### Option A — Docker (recommended, no local SQL Server needed)

```bash
docker compose up --build
```

The API is then available at `http://localhost:8080` and Swagger at
`http://localhost:8080/swagger`.

### Option B — Run locally with the .NET SDK

Requires the .NET 10 SDK and a reachable SQL Server. Set the connection string and
JWT settings in `appsettings.Development.json`, then:

```bash
dotnet restore
dotnet run
```

Swagger will be available at the URL printed in the console (e.g. `https://localhost:7xxx/swagger`).

### Seeded test user

On first run the database is seeded with a demo account so you can log in immediately:

| Username | Password |
|---|---|
| `testuser` | `test123` |

---

## API reference

Base path: `/api`

### Auth — `/api/auth`

| Method | Route | Body | Description |
|---|---|---|---|
| `POST` | `/register` | `{ "userName", "password" }` | Create a new account. |
| `POST` | `/login` | `{ "userName", "password" }` | Returns a JWT string on success. |

### Tasks — `/api/tasks` *(requires `Authorization: Bearer <token>`)*

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | List the current user's tasks. |
| `GET` | `/{id}` | Get one task by id. |
| `POST` | `/` | Create a task. |
| `PUT` | `/{id}` | Update a task. |
| `DELETE` | `/{id}` | Delete a task. |

### Example flow

```bash
# 1. Log in and capture the token
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"testuser","password":"test123"}')

# 2. Create a task
curl -X POST http://localhost:8080/api/tasks \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Write README"}'

# 3. List your tasks
curl http://localhost:8080/api/tasks -H "Authorization: Bearer $TOKEN"
```

---

## Running the tests

```bash
dotnet test
```

The suite verifies task creation, per-user filtering, owner-checked deletion, and
updates using an in-memory database (no SQL Server required).

---

## Security notes

- The credentials in `docker-compose.yml` and `appsettings.json` (SQL password,
  `Jwt:Key`) are **local development values only**. In a real deployment they would
  be supplied as environment variables / secrets and never committed.
- HTTPS redirection, token-expiry validation, and issuer/audience validation are all enabled.

## Possible next steps

- Refresh tokens and role-based authorization (an admin `Role` field already exists on the user model).
- Pagination and filtering on the task list.
- CI pipeline (build + test) via GitHub Actions.
