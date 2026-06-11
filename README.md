# Clinic Management System 2.0

A modern, robust Clinic Management System built with ASP.NET Core 10, Entity Framework Core (Code First), and Microsoft SQL Server. The project uses Docker for database hosting, Razor Pages/Views + Bootstrap 5 for the frontend, and NLog for logging.

## Usage Guide

### Roles & Permissions

The system has three roles, each with a distinct set of permissions:

| Feature | Admin | Doctor | Receptionist |
|---------|-------|--------|-------------|
| Patient registry (view, create, edit, soft-delete) | ✅ | ✅ | ✅ |
| Visit calendar & appointment booking | ✅ | ✅ | ✅ |
| Manage visit (procedures, prescriptions, notes) | ✅ | ✅ | ❌ |
| Medication catalog (add, edit, toggle availability) | ✅ | ❌ | ✅ |
| Medical procedures catalog (add, edit) | ✅ | ❌ | ❌ |
| User management (list users, manage roles, create staff, delete users) | ✅ | ❌ | ❌ |
| Download visit summary & prescription PDFs | ✅ | ✅ | ❌ |

### Screens Overview

#### Home Page (`/`)
Dashboard with role-tailored quick links to all available modules.

#### Patient Registry (`/Patient`)
Available to Admin, Doctor, and Receptionist. Search patients by name or PESEL, view details, create new patients, edit existing records, and soft-delete patients (the record and its visit history are preserved).

#### Visit Calendar (`/Visit/Calendar`)
Available to Admin, Doctor, and Receptionist. Select a doctor and date to view their daily schedule. Book new appointments from available 30-minute time slots. Advance visits through the status lifecycle:
- Scheduled → Start → In Progress → Complete → Completed
- Scheduled or In Progress → Cancel → Cancelled

#### Manage Visit (`/Visit/Manage/{id}`)
Available to Admin and Doctor when a visit is **In Progress**. Add medical procedures from the catalog (cost is auto-calculated from the procedure's service price). Prescribe medications (cost = unit price × quantity). Write clinical notes categorized as History, Diagnosis, or Recommendations. Total cost is recalculated automatically on every change.

#### Medication Catalog (`/Medication`)
Available to Admin and Receptionist. View, add, edit medications, and toggle their availability status.

#### Medical Procedures (`/MedicalProcedure`)
Available to Admin only. View, add, and edit the clinic's procedure catalog and pricing.

#### Staff Management (`/Admin/Users`)
Available to Admin only. View all users with their assigned roles. Create new staff accounts (password is auto-generated and shown once with a copy button). Manage individual user roles via checkboxes. Delete users (blocked if they have associated records).

#### Download PDFs
For **completed** visits, a Doctor or Admin can download:
- **Visit Summary** (`/Visit/DownloadSummary/{id}`) - includes patient info, visit details, procedures, medications, and all clinical notes.
- **Prescription** (`/Visit/DownloadPrescription/{id}`) - includes patient info and prescribed medications with dosage.


### Seeded Accounts

On first run, the application seeds the following test accounts with predefined roles:

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@clinic.com` | `Admin@2026!` |
| Doctor | `dr.kowalski@clinic.com` | `ClinicSecure2026!` |
| Receptionist | `anna.nowak@clinic.com` | `ClinicSecure2026!` |

### How to Create Staff Accounts

1. Log in as Admin.
2. Navigate to **Clinic Staff** in the top navigation bar.
3. Click **+ Create Staff**.
4. Fill in the staff member's first name, last name, email, and select their role (Admin, Doctor, or Receptionist).
5. Click **Create Staff Account**.
6. A secure password is generated automatically. Copy it immediately using the **📋 Copy** button - the password is only shown once.

### Navigation

The top navigation bar adapts based on the logged-in user's role:
- **Patients** - visible to all authenticated users.
- **Visits** - visible to all authenticated users.
- **Medications** - visible to Admin and Receptionist.
- **Medical Procedures** - visible to Admin only.
- **Clinic Staff** - visible to Admin only.

## Project Structure

The project is structured as follows:

*   **`ClinicManager/`**: Main ASP.NET Core MVC web application.
    *   `Controllers/`: MVC Controllers.
    *   `DTOs/`: Data Transfer Objects for clean request/response separation.
    *   `Models/`: Domain Entities (Patient, Visit, ClinicalNote, etc.).
    *   `Services/`: Business Logic Layer (BLL) services.
    *   `Mappers/`: Object mapping using Mapperly.
    *   `Views/`: Razor Views for presentation.
    *   `BackgroundServices/`: Asynchronous background processing.
    *   `Data/`: Entity Framework Core DbContext, migrations, and seeding logic.
    *   `wwwroot/uploads/`: Document scan uploads (e.g. medical record scans).
    *   `Logs/`: NLog log file directory.
*   **`ClinicManager.Tests/`**: Unit test suite using xUnit, Moq, and EF Core InMemory database.
*   **`ClinicManager.PerformanceTests/`**: Performance load testing suite using NBomber.

## Technologies Used

*   **Runtime**: .NET 10.0
*   **Web Framework**: ASP.NET Core MVC
*   **ORM**: Entity Framework Core 10 (SQL Server Provider)
*   **Database**: Microsoft SQL Server (Docker-based)
*   **Logging**: NLog
*   **Mapping**: Mapperly
*   **PDF Generation**: QuestPDF
*   **Performance Testing**: NBomber
*   **CI/CD**: GitHub Actions

## CI/CD Pipeline

Two GitHub Actions workflows run on every push and pull request to the `main` branch:

*   **Build and test** (`dotnet-ci.yml`): Restores dependencies, builds the solution, and runs the full unit test suite (`dotnet test`).
*   **Check Code Formatting** (`format-check.yml`): Verifies that all code follows the project's formatting conventions using `dotnet format --verify-no-changes`.

Both workflows use the .NET 10 SDK on `ubuntu-latest`.

## Setup & Running

### Prerequisites

*   .NET 10 SDK
*   Docker & Docker Compose (for SQL Server database)

### Initial Setup (First Time Only)

Before running the application for the first time, restore the local repository development tools

1. Restore the local tools defined in the repository manifest:
   ```bash
   dotnet tool restore
   ```

---

### How to Run

1. **Start the Database Infrastructure:** Spin up the containerized SQL Server instance via Docker Compose (the `docker-compose.yml` file is located in the solution root):
   ```bash
   docker compose up -d
   ```

2. **Restore Dependencies:** Pull down the required NuGet packages (including NLog, EF Core, and Identity):
   ```bash
   dotnet restore
   ```

3. **Run the Application:** Launch the web server. The application automatically handles pending EF Core schema migrations and runs the master system data seeder (`DataSeeder`) upon startup:
   ```bash
   dotnet run --project ClinicManager
   ```

For testing the email sevice:
```bash
docker run -p 1080:1080 -p 1025:1025 maildev/maildev 
```

To run tests:
```bash
dotnet test
```

To run performance tests (NBomber):
```bash
# start the database
docker compose up -d

dotnet run --project ClinicManager --urls=http://localhost:5000/
dotnet run --project ClinicManager.PerformanceTests
```
