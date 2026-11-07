# AlterTrack: plan

A work-order system for a tailoring and alterations shop. Staff log garments in at intake,
track each order through fittings to pickup, and see what is due. Managers get a weekly summary.

**Why this project:** it is a normal business app (CRUD, a relational database, business rules,
auth, a frontend, tests, CI, deployment), which is what most entry-level jobs involve. The
domain is one you know from real work, so you can explain *why* each feature exists.

**How we work:** you write the code. Claude sets the plan, explains concepts, reviews each
milestone and helps when you're stuck. Commit at the end of every milestone at minimum, with a
message that says what changed and why. A steady commit history is part of what recruiters see.

---

## Stack

| Layer | Choice | Why |
|---|---|---|
| API | ASP.NET Core 10 Web API (controllers) | Most common .NET job stack; .NET 10 is the current LTS |
| Database | PostgreSQL 17 + Entity Framework Core (Npgsql) | Relational schema + migrations |
| Tests | xUnit, `WebApplicationFactory` integration tests against a real test database | Tests that hit real SQL catch real bugs |
| Auth | ASP.NET Core Identity, roles `Staff` and `Manager` | Built-in, standard |
| Frontend | React + TypeScript (Vite) | Most-requested frontend skill |
| CI | GitHub Actions (build + test, with a Postgres service container) | Every push is checked |
| Hosting | Decide at milestone 7 (Azure App Service, or Render + Neon free Postgres) | |

## Data model (first version)

- **Customer**: Id, Name, Phone, Email (optional), Notes, CreatedAt
- **Order**: Id, CustomerId, Status, DueDate, IsRush, CreatedAt, total price (sum of items)
- **OrderItem**: Id, OrderId, GarmentType (e.g. Suit jacket), AlterationType (e.g. Hem), Description, Price
- **StatusChange**: Id, OrderId, FromStatus, ToStatus, ChangedByUserId, ChangedAt (audit history)

**Order status flow** (the business rule interviewers will ask about):

```
Received -> AwaitingFitting -> InProgress -> ReadyForPickup -> PickedUp
    any status except PickedUp -> Cancelled
```

Any other move (for example `PickedUp -> InProgress`) is rejected with HTTP 409.

---

## Milestones

Each one ends with: code builds, tests pass, committed and pushed.

### M0: Setup (about 1 evening)
Goal: an empty API and test project that build, in a Git repo on GitHub.

1. Install the tools (run in a terminal; each opens an installer). Remember the Postgres
   password you choose, and **never commit it**.
   ```
   winget install Microsoft.DotNet.SDK.10
   winget install PostgreSQL.PostgreSQL.17
   ```
   Then close and reopen the terminal and check: `dotnet --list-sdks` shows 10.x.
2. Create the solution (run inside `E:\Projects\AlterTrack`). Read what each line does:
   ```
   git init
   dotnet new gitignore
   dotnet new sln -n AlterTrack
   dotnet new webapi --use-controllers -o src/AlterTrack.Api
   dotnet new xunit -o tests/AlterTrack.Api.Tests
   dotnet sln add src/AlterTrack.Api tests/AlterTrack.Api.Tests
   dotnet add tests/AlterTrack.Api.Tests reference src/AlterTrack.Api
   dotnet add src/AlterTrack.Api package Npgsql.EntityFrameworkCore.PostgreSQL
   dotnet add src/AlterTrack.Api package Microsoft.EntityFrameworkCore.Design
   dotnet tool install --global dotnet-ef
   dotnet build
   dotnet test
   ```
3. Delete the template's `WeatherForecast` files, build again, make your first commit.
4. Create the GitHub repo (public) and push: `gh repo create AlterTrack --public --source . --push`

**Interview check:** what is a solution vs a project? What does `dotnet add reference` do?

### M1: Customers (about 2–3 evenings)
Goal: create, list, get, update and delete customers, stored in Postgres.
- `Customer` entity, `AppDbContext`, connection string in **user secrets** (`dotnet user-secrets`), not in `appsettings.json`.
- First migration: `dotnet ef migrations add InitialCreate`, then `dotnet ef database update`.
- `CustomersController` with request/response DTOs (don't return entities directly) and validation (Name and Phone required).
- First integration tests: create then fetch a customer; missing name returns 400.
- Add the GitHub Actions workflow so tests run on every push.

**Interview check:** why DTOs? What is a migration? Why user secrets?

### M2: Orders and the status flow (about 1 week)
Goal: the core business logic, well tested.
- `Order`, `OrderItem`, `StatusChange` entities and a migration.
- `POST /orders` (with items), `GET /orders/{id}`, `POST /orders/{id}/status`.
- Put the transition rules in one place (a small class with a method like `CanMoveTo`) and unit-test every allowed and rejected transition.
- Each status change writes a `StatusChange` row.

**Interview check:** where does business logic belong, and why not in the controller?

### M3: The due-date queue (about 3–4 evenings)
- `GET /orders?status=&dueBefore=&customer=&page=&pageSize=` with filtering, sorting by due date (rush first) and pagination.
- Add database indexes for the columns you filter on; be ready to explain why.

### M4: Auth and roles (about 1 week)
- ASP.NET Core Identity, login endpoint, `Staff` and `Manager` roles.
- Staff can do everything except cancel orders and see reports; Managers can do both.
- Status changes record who made them.
- Tests: an anonymous request returns 401, Staff cancelling returns 403.

### M5: React frontend (about 1–2 weeks)
- `npm create vite@latest web -- --template react-ts`
- Pages: login, order queue (the home screen), new order, order detail with status buttons and history, customers.

### M6: Weekly summary (about 2–3 evenings)
- Manager-only `GET /reports/weekly`: orders completed, revenue, average turnaround in days, overdue count.
- A dashboard page showing it.

### M7: Ship it (about 1 week)
- Deploy the API, the database and the frontend; add seed demo data and a demo login.
- README with screenshots, a live link, how to run locally, and an architecture diagram.
- Then fill in the resume bullets below with real numbers.

---

## Resume bullets (fill in at M7, real numbers only)

- Built a full-stack work-order system (ASP.NET Core, PostgreSQL, React/TypeScript) that tracks garment alterations from intake to pickup, with role-based access for staff and managers.
- Modeled the order lifecycle as a validated state machine with a full audit history; covered it with [N] unit and integration tests that run in GitHub Actions on every push.
- Deployed to [host] with seeded demo data; [one more concrete result, e.g. a filtered due-date queue with pagination and indexes].

## Next projects (after this one)

2. Public-data pipeline and dashboard (Python, PostgreSQL, FastAPI or Streamlit)
3. Job-application tracker API (Java, Spring Boot, PostgreSQL, JUnit)
