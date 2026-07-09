# GymManager - Fitness Club Management System
Polish version: [README_PL.md](README_PL.md)

A single-page web application developed as an engineering thesis project for a Software Engineering degree.

The project was defended with the **highest grade and distinction**.

GymManager is designed to support fitness club management, including user accounts, memberships, payments, class reservations, club entries, announcements, and an administrative panel.

The application follows a client-server architecture:

- **ASP.NET Core 8 Web API** — backend
- **Blazor WebAssembly** — frontend
- **SQL Server 2022** — database running in a Docker container
- **Entity Framework Core** — ORM and database migrations
- **JWT Bearer** — authentication and authorization

---

## Table of Contents

1. [Tech Stack](#tech-stack)
2. [Features](#features)
3. [System Requirements](#system-requirements)
4. [Tool Installation](#tool-installation)
5. [Running the Database](#running-the-database)
6. [Running the Application](#running-the-application)
7. [First Login and Administrator Account](#first-login-and-administrator-account)
8. [Project Structure](#project-structure)
9. [API Endpoints](#api-endpoints)
10. [Troubleshooting](#troubleshooting)
11. [Author](#author)

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Backend | ASP.NET Core Web API | 8.0 |
| Frontend | Blazor WebAssembly | 8.0 |
| ORM | Entity Framework Core | 9.0 |
| Database | Microsoft SQL Server | 2022 |
| Containerization | Docker Compose | v2 |
| Authentication | JWT Bearer | - |
| Password Hashing | PBKDF2 | - |
| API Documentation | Swagger / OpenAPI | 3.0 |

> The project targets **.NET 8**, while Entity Framework Core packages use version **9.x**.

---

## Features

### User

- User registration and login
- JWT authentication with refresh tokens
- User profile management
- User data export
- Account deletion
- Membership purchase
- Active and historical membership overview
- Group class schedule
- Class reservation and cancellation
- Club entry history
- QR code support
- Announcements overview

### Administrator

- User list overview
- User account details
- Membership assignment and cancellation
- Class type management
- Class session management
- Attendance list overview
- Manual entry registration
- User entry verification
- Announcement management
- Publishing and hiding announcements

---

## System Requirements

| Requirement | Description |
|---|---|
| Operating system | Windows 10/11 64-bit, macOS, or Linux |
| RAM | Minimum 8 GB |
| Free disk space | Around 5 GB |
| Browser | Chrome, Edge, or Firefox with WebAssembly support |
| Docker | Required to run SQL Server |
| .NET SDK | Version 8.x |

SQL Server running in Docker requires approximately 2 GB of RAM.

---

## Tool Installation

A clean environment requires three tools:

1. .NET 8 SDK
2. Docker Desktop
3. Entity Framework Core CLI

---

### 1. .NET 8 SDK

The SDK includes the compiler, the `dotnet` CLI tools, and the runtime environment.

1. Download and install `.NET SDK 8.0` from Microsoft.
2. Run the installer and follow the setup wizard.
3. Open a new terminal and verify the installation:

```bash
dotnet --version
```

Expected result:

```text
8.0.xxx
```

---

### 2. Docker Desktop

Docker is required to run SQL Server 2022 in a container.

1. Download and install Docker Desktop.
2. On Windows, enabling WSL 2 may be required.
3. Restart the computer after installation.
4. Start Docker Desktop and wait until the status is set to `Running`.

Verify the installation:

```bash
docker --version
docker compose version
```

Expected result:

```text
Docker version 2x.x.x
Docker Compose version v2.x.x or newer
```

If Docker Desktop does not start and displays a virtualization-related error, enable virtualization in BIOS/UEFI. The option is usually called `Intel VT-x` or `AMD-V`.

---

### 3. Entity Framework Core CLI

After installing the .NET SDK, install the global EF Core CLI tool in version 9.x:

```bash
dotnet tool install --global dotnet-ef --version 9.0.*
```

Check the version:

```bash
dotnet ef --version
```

Expected result:

```text
9.0.x
```

If a different version is installed, for example 10.x, uninstall the tool and install the correct version again:

```bash
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef --version 9.0.*
```

---

## Running the Database

### 1. Starting the SQL Server Container

Open a terminal in the root directory of the solution, where the `docker-compose.yml` file is located, and run:

```bash
docker compose up -d
```

This command downloads the SQL Server 2022 image and starts a container named:

```text
gym_sql
```

The database will be available on port:

```text
1433
```

Check whether the container is running:

```bash
docker ps
```

The output should include `gym_sql` with the `Up` status.

---

### 2. Applying Database Migrations

Go to the API project directory:

```bash
cd GymManager.Api
```

Apply the migrations:

```bash
dotnet ef database update
```

This command creates the database:

```text
GymManagerDb
```

and all required tables, including:

- `Users`
- `Passes`
- `Payments`
- `PassTypes`
- `ClassTypes`
- `ClassSessions`
- `ClassReservations`
- `Entries`
- `RefreshTokens`
- `Announcements`

Expected final result:

```text
Done.
```

---

## Running the Application

The application consists of two projects that must be started at the same time in separate terminal windows:

- `GymManager.Api` — backend
- `GymManager.Client` — frontend

---

### 1. Running the API

In the first terminal, go to the API directory:

```bash
cd GymManager.Api
```

Run the project:

```bash
dotnet run --launch-profile https
```

The API will be available at:

```text
https://localhost:7048
http://localhost:5048
```

Swagger UI will be available at:

```text
https://localhost:7048/swagger
```

On the first run, .NET may generate a local HTTPS development certificate. If the browser displays a certificate warning, you can accept the exception or trust the certificate globally:

```bash
dotnet dev-certs https --trust
```

---

### 2. Running the Blazor Client

In the second terminal, go to the client directory:

```bash
cd GymManager.Client
```

Run the project:

```bash
dotnet run --launch-profile https
```

The frontend will be available at:

```text
https://localhost:7132
```

Open this address in your browser.

---

### 3. Quick Start

Terminal 1 — database and migrations:

```bash
docker compose up -d

cd GymManager.Api
dotnet ef database update
dotnet run --launch-profile https
```

Terminal 2 — client:

```bash
cd GymManager.Client
dotnet run --launch-profile https
```

Browser:

```text
https://localhost:7132
```

---

## First Login and Administrator Account

### 1. User Registration

1. Open the application at:

```text
https://localhost:7132
```

2. Click `Register`.
3. Fill in the registration form.
4. Accept the terms and privacy policy.
5. After registration, the user will be logged in automatically.

A newly created account receives the default role:

```text
user
```

---

### 2. Granting Administrator Role

To grant administrator privileges to a user, update the user role directly in the database.

Connect to the database using `sqlcmd` inside the Docker container:

```bash
docker exec -it gym_sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Change_This_Password_123!" -C -Q "UPDATE GymManagerDb.dbo.Users SET Role = 'admin' WHERE Email = 'your@email.com';"
```

Replace:

```text
your@email.com
```

with the email address of the account that should become an administrator.

After changing the role, log out and log in again. The new JWT token will contain the updated role.

---

### 3. Adding Membership Types

To allow users to buy memberships, membership types must be added to the database.

Example membership types can be inserted with the following command:

```bash
docker exec -it gym_sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Change_This_Password_123!" -C -Q "INSERT INTO GymManagerDb.dbo.PassTypes (PassTypeId,Name,Description,DurationDays,Price,Currency,IsActive,CreatedAt) VALUES (NEWID(),'Monthly','30-day membership',30,99.00,'PLN',1,SYSUTCDATETIME()),(NEWID(),'Quarterly','90-day membership',90,249.00,'PLN',1,SYSUTCDATETIME()),(NEWID(),'Half-year','180-day membership',180,449.00,'PLN',1,SYSUTCDATETIME()),(NEWID(),'Yearly','365-day membership',365,799.00,'PLN',1,SYSUTCDATETIME());"
```

---

## Project Structure

```text
GymManager/
  docker-compose.yml
  GymManager.sln

  GymManager.Api/
    Program.cs
    appsettings.json
    appsettings.Development.json

    Controllers/
      AuthController.cs
      ProfileController.cs
      PassesController.cs
      PassTypesController.cs
      ClassesController.cs
      EntriesController.cs
      AnnouncementsController.cs
      AdminUsersController.cs
      AdminPassesController.cs

    Data/
      AppDbContext.cs
      Configurations/

    Domain/
      Entities/
        User.cs
        RefreshToken.cs
        Pass.cs
        Payment.cs
        PassType.cs
        ClassType.cs
        ClassSession.cs
        ClassReservation.cs
        Entry.cs
        Announcement.cs

    Services/
      Security/
        PasswordService.cs
        JwtTokenService.cs
        RefreshTokenService.cs

    Dtos/
    Migrations/

  GymManager.Client/
    Program.cs
    App.razor

    Layout/
      MainLayout.razor

    Pages/
      Home.razor
      Login.razor
      Register.razor
      Profile.razor
      PassTypes.razor
      MyPasses.razor
      ClassesSchedule.razor
      CheckIn.razor
      MyQrCode.razor
      MyEntries.razor
      Announcements.razor

      Admin/
        AdminUsers.razor
        AdminUserDetails.razor
        AdminClasses.razor
        AdminScanner.razor
        AdminEntries.razor
        AnnouncementsAdmin.razor

    Services/
      ApiHttpClient.cs

      Auth/
        TokenStore.cs
        AuthApiClient.cs
        AuthorizedHandler.cs
        JwtHelper.cs

    Contracts/
    wwwroot/
```

---

## API Endpoints

Full interactive API documentation is available after starting the backend:

```text
https://localhost:7048/swagger
```

---

### Public Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/auth/register` | Registers a new user |
| POST | `/auth/login` | Logs in a user |
| POST | `/auth/refresh` | Refreshes the access token |
| POST | `/auth/logout` | Logs out a user |

---

### User Endpoints

Authentication is required.

| Method | Endpoint | Description |
|---|---|---|
| GET | `/auth/me` | Returns current user data |
| GET | `/profile` | Returns user profile data |
| PUT | `/profile` | Updates user profile |
| GET | `/profile/export` | Exports user data |
| DELETE | `/profile` | Deletes user account |
| GET | `/passtypes` | Returns available membership types |
| GET | `/passes/me` | Returns user memberships |
| POST | `/passes/buy` | Initializes membership purchase |
| POST | `/passes/confirm` | Confirms payment |
| GET | `/classes/types` | Returns active class types |
| GET | `/classes/schedule` | Returns class schedule |
| POST | `/classes/sessions/{id}/reserve` | Reserves a class |
| DELETE | `/classes/sessions/{id}/reserve` | Cancels a reservation |
| GET | `/classes/me` | Returns upcoming user reservations |
| POST | `/entries/checkin` | Self-service club entry |
| GET | `/entries/me` | Returns user entry history |
| GET | `/announcements` | Returns published announcements |

---

### Administrator Endpoints

Administrator role is required.

| Method | Endpoint | Description |
|---|---|---|
| GET | `/admin/users` | Returns user list |
| GET | `/admin/users/{id}` | Returns user details |
| POST | `/admin/passes/assign` | Assigns a membership |
| POST | `/admin/passes/cancel` | Cancels a membership |
| GET | `/classes/types/all` | Returns all class types |
| POST | `/classes/types` | Adds a class type |
| PUT | `/classes/types/{id}` | Updates a class type |
| DELETE | `/classes/types/{id}` | Deletes a class type |
| POST | `/classes/sessions` | Adds a class session |
| PUT | `/classes/sessions/{id}` | Updates a class session |
| PATCH | `/classes/sessions/{id}/cancel` | Cancels or restores a session |
| DELETE | `/classes/sessions/{id}` | Deletes a session |
| DELETE | `/classes/sessions/{sid}/reservations/{uid}` | Removes a user from a class |
| GET | `/classes/sessions/{id}/attendance` | Returns attendance list |
| GET | `/entries` | Returns all entry history |
| POST | `/entries/verify` | Verifies user QR code |
| POST | `/entries/manual` | Registers manual entry |
| GET | `/announcements/admin` | Returns all announcements |
| POST | `/announcements` | Adds an announcement |
| PATCH | `/announcements/{id}/publish` | Publishes or hides an announcement |
| DELETE | `/announcements/{id}` | Deletes an announcement |

---

## Troubleshooting

### Docker container does not start

Check container logs:

```bash
docker logs gym_sql
```

The most common reason is not enough memory assigned to Docker Desktop. SQL Server requires approximately 2 GB of RAM.

In Docker Desktop, check:

```text
Settings -> Resources
```

---

### Docker Desktop does not start after installation

If a virtualization or WSL-related error appears:

1. Restart the computer.
2. Enter BIOS/UEFI.
3. Enable virtualization:
   - `Intel VT-x` for Intel processors
   - `AMD-V` for AMD processors
4. Save the settings and restart the computer.

---

### Database connection error during migration

Example message:

```text
A connection was successfully established with the server, but then an error occurred
```

Check whether the container is running:

```bash
docker ps
```

If the container has just started, SQL Server may need several seconds to initialize. Wait a moment and try again.

---

### Browser does not load the client correctly

Make sure both projects are running at the same time:

- `GymManager.Api`
- `GymManager.Client`

The Blazor client requires the API to work correctly.

---

### HTTPS certificate warning

On the first run in development mode, .NET generates a self-signed certificate.

To trust the local certificate:

```bash
dotnet dev-certs https --trust
```

Then restart the browser.

---

### Port 1433 is already in use

If another process, such as a local SQL Server instance, is already using port `1433`, change the port mapping in `docker-compose.yml`:

```yml
ports:
  - "1434:1433"
```

Then update the connection string in `appsettings.Development.json`:

```text
Server=localhost,1434;...
```

---

### Database reset

To remove the container and its data, then recreate the database:

```bash
docker compose down -v
docker compose up -d

cd GymManager.Api
dotnet ef database update
```

---

### `Unable to retrieve project metadata` error

The most common reason is an incompatible version of the `dotnet-ef` tool.

Check the version:

```bash
dotnet ef --version
```

If the result is `10.x` or any version other than `9.0.x`, uninstall the tool and install the correct version:

```bash
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef --version 9.0.*
```

---

## Author

**Rafał Bartczak**  
Engineering thesis project, 2026  
Field of study: Software Engineering
