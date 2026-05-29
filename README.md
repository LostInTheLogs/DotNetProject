# Clinic Management System 2.0

A modern, robust Clinic Management System built with ASP.NET Core 10, Entity Framework Core (Code First), and Microsoft SQL Server. The project uses Docker for database hosting, Razor Pages/Views + Bootstrap 5 for the frontend, and NLog for logging.

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

## Setup & Running

### Prerequisites

*   .NET 10 SDK
*   Docker & Docker Compose (for SQL Server database)

### How to Run

1.  Start the SQL Server database using Docker Compose (details in Faza 1 Commit 3).
2.  Restore dependencies:
    ```bash
    dotnet restore
    ```
3.  Apply migrations and start the application:
    ```bash
    dotnet run --project ClinicManager
    ```