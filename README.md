# Secure Task API

A multi-user to-do REST API in ASP.NET Core where every task is locked to the user who created it.

I built this to get comfortable with the security side of a .NET backend rather than just CRUD. The interesting constraint I set for myself: a logged-in user should never be able to see or touch another user's tasks, even if they guess the ID. That one rule ended up shaping most of the design — the owner is always taken from the JWT, never from the request body or the URL.

## What it does

You register, you log in, you get a JWT. From then on every call to `/api/tasks` is filtered by the user ID baked into that token's `NameIdentifier` claim. The controller never trusts a user-supplied owner ID, so asking for `GET /api/tasks/5` only returns task 5 if it's actually yours — otherwise you get a 404, not someone else's data.

A few specifics worth calling out:

- **JWT bearer auth** via `Microsoft.AspNetCore.Authentication.JwtBearer`, with issuer, audience, lifetime and signing-key validation all switched on.
- **Passwords hashed with BCrypt** (`BCrypt.Net-Next`) — nothing is ever stored in plain text.
- **A service layer** (`ITaskService` / `TaskService`) sitting between the controllers and EF Core, so the per-user filtering logic is testable without a web server or a real database.
- **EF Core 10 + SQL Server**, with `db.Database.Migrate()` running on startup and a demo user seeded the first time the DB is empty.
- **Swagger UI** with the Authorize button wired up, so you can paste a token and hit the protected endpoints from the browser.
- A small **global exception handler** that returns a plain JSON error instead of a stack trace.

## Stack

| | |
|---|---|
| Framework | ASP.NET Core / .NET 10 |
| Auth | JWT bearer tokens |
| Hashing | BCrypt.Net-Next |
| Data | EF Core 10 (SQL Server) |
| Docs | Swashbuckle / Swagger |
| Tests | xUnit + EF Core In-Memory |
| Container | Docker + Docker Compose |

The flow is roughly: `Controllers -> ITaskService -> AppDbContext -> SQL Server`, with `JwtTokenService` handling token creation off to the side.

## Running it

The quickest path is Docker — it brings up the API and a SQL Server 2022 container together, so you don't need a local SQL install:

```bash
docker compose up --build
```

API on `http://localhost:8080`, Swagger at `http://localhost:8080/swagger`.

To run it directly with the SDK instead, you need the .NET 10 SDK and a reachable SQL Server. The default connection string in `appsettings.json` points at LocalDB. Then:

```bash
dotnet restore
dotnet run
```

That serves on `http://localhost:5028` (and `https://localhost:7242`) per `launchSettings.json`.

On first run the DB is seeded with a demo account so you can log in straight away:

```
username: testuser
password: test123
```

## Endpoints

Auth (`/api/auth`):

- `POST /register` — `{ "userName", "password" }`, creates an account.
- `POST /login` — same body, returns a JWT in the response.

Tasks (`/api/tasks`, all require `Authorization: Bearer <token>`):

- `GET /` — your tasks
- `GET /{id}` — one task (404 if it isn't yours)
- `POST /` — create
- `PUT /{id}` — update
- `DELETE /{id}` — delete

Quick smoke test against the Docker setup:

```bash
# Log in — the response body is the JWT (as a JSON string)
curl -s -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"testuser","password":"test123"}'

# Then use that token (strip the surrounding quotes) for task calls
curl -X POST http://localhost:8080/api/tasks \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Write the README"}'
```

## Tests

```bash
dotnet test SecureTaskApi.Tests/SecureTaskApi.Tests.csproj
```

The suite is small but it covers the parts I actually cared about: creating a task, the per-user filter (user 1 can't see user 2's task), an owner-checked delete that fails for the wrong user, and an update. It runs on EF Core's in-memory provider, so no SQL Server needed. There's also a GitHub Actions workflow (`.github/workflows/ci.yml`) that restores, builds, runs the tests and builds the Docker image on every push and PR to `main`.

## A note on the committed secrets

The SQL SA password and `Jwt:Key` in `docker-compose.yml` and `appsettings.json` are local development throwaways, deliberately committed so the project runs out of the box. In anything real they'd come from environment variables or a secret store and would never be in the repo.

## What I'd add next

- Refresh tokens, and role-based authorization — there's already a `Role` field on the user model that nothing uses yet.
- Pagination and filtering on the task list once it grows past a handful of rows.