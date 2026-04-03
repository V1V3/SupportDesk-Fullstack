# SupportDesk.Api

SupportDesk.Api is a production-style ASP.NET Core 8 Web API for ticket management, built and deployed to demonstrate modern backend development practices including authentication, authorization, database integration, and API design.

This project was built to bridge the gap between basic CRUD applications and real-world backend systems by incorporating authentication, authorization, database persistence, structured logging, and deployment.

## Live Demo

Swagger UI:
https://supportdesk-5pob.onrender.com/swagger

Demo Credentials (Admin account with full permissions):

Username: demo  
Password: Pass123!

Users can also register and log in to their own created accounts and authorize them to interact with the API.

## Key Concepts Demonstrated

- RESTful API design
- Layered architecture (controllers + services + DTOs)
- Secure authentication with JWT
- Role-based and ownership-based authorization
- Database modeling and migrations with EF Core
- Query optimization with pagination, filtering, and sorting
- Production-ready deployment using Docker and Render

## Preview

<img width="1441" height="936" alt="image" src="https://github.com/user-attachments/assets/f98e2820-e10a-42dc-9710-fd56d79060e6" />


## Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core 8
- PostgreSQL (Render hosted)
- JWT Authentication
- Serilog (structured logging)
- xUnit (integration testing)
- Docker (deployment)

## Features

### Authentication & Users
- User registration with hashed passwords
- JWT-based authentication
- Current user endpoint

### Ticket System
- Full CRUD operations
- Ownership-based authorization
- Admin override permissions

### Querying & Performance
- Pagination
- Filtering (status, creator, search)
- Sorting (multiple fields)

### Reliability & Observability
- Structured logging with Serilog
- Integration tests using WebApplicationFactory

## Deployment

The API is containerized with Docker and deployed on Render.

- Automatic database migrations on startup
- Connected to managed PostgreSQL instance
- Publicly accessible via Swagger UI

## Authorization Rules

- Authenticated users can create tickets
- Ticket owners can update and delete their own tickets
- Admin users can update and delete any ticket

## Ticket Query Options

`GET /api/tickets` supports:

- `page`
- `pageSize`
- `status`
- `search`
- `createdBy`
- `sortBy`
- `sortDirection`

Supported `sortBy` values:

- `createdAtUtc`
- `id`
- `title`
- `status`
- `createdBy`

Supported `sortDirection` values:

- `asc`
- `desc`

## Run Locally

1. Clone the repository
2. Configure PostgreSQL connection in `appsettings.Development.json`
3. Apply migrations:

   dotnet ef database update

4. Run the API:

   dotnet run
