# Student Grade Management System

## 1. Project Overview

The Student Grade Management System is a web-based application developed using ASP.NET Core Web API and ASP.NET Core MVC. It allows faculty members to manage student grade records while students can securely view their own grades.

The system uses role-based authentication and authorization to protect grade information and ensure that students can only access their own academic records.

## 2. Features

### Faculty
- Log in using a Faculty account.
- View student grade records.
- Add new grade records.
- Update existing grade records.
- Delete grade records.
- View automatically computed grade remarks.

### Student
- Register using an existing Student Number.
- Log in using a Student account.
- View personal grade records.
- Access only their own grades.

### System Features
- JWT authentication.
- Role-based authorization for Faculty and Student accounts.
- Server-side grade validation.
- Automatic grade remark computation.
- SQL Server database integration.
- Asynchronous API requests using JavaScript Fetch API.

## 3. Technologies Used

- ASP.NET Core Web API
- ASP.NET Core MVC
- C#
- JavaScript (Fetch API, async/await)
- HTML and CSS
- Bootstrap 5
- JSON
- SQL Server
- Entity Framework Core
- ASP.NET Core Identity
- JWT Authentication
- Postman
- Visual Studio 2026

## 4. Prerequisites

Before running the application, install the following:

- Visual Studio 2026
- .NET 10 SDK
- SQL Server
- SQL Server Management Studio (SSMS)
- Postman

## 5. Database Setup

The project uses SQL Server with the database name:

`StudentGradeManagementDB`

### Step 1: Create the Academic Database

1. Open SQL Server Management Studio.
2. Connect to your local SQL Server instance using Windows Authentication.
3. Open the SQL script located in the `Database` folder:

   `StudentGradeManagementDB_WS_FINAL_PROJECT.sql`

4. Execute the script to create the academic database, tables, constraints, stored procedure, and sample records.

**Important:** This script is intended for setting up a fresh database. Do not execute it again on an existing database containing records you want to preserve.

### Step 2: Configure the Connection String

The API connection string is configured in `appsettings.json`.

Example:

`Server=localhost;Database=StudentGradeManagementDB;Integrated Security=True;TrustServerCertificate=True;`

Update the server name if your SQL Server instance uses a different configuration.

### Step 3: Apply Identity Migration

The project uses a separate `AuthDbContext` for ASP.NET Core Identity authentication tables.

Open the Package Manager Console in Visual Studio and execute:

`Update-Database -Context AuthDbContext -Project StudentGradeManagement.API -StartupProject StudentGradeManagement.API`

This applies the Identity migration without requiring Entity Framework migrations to recreate the existing academic tables.

## 6. Authentication and Account Seeding

The application uses ASP.NET Core Identity and JWT authentication.

The system automatically seeds the following roles when the API starts:

- Student
- Faculty

A demo Faculty account is also created through the application's seeding process.

Student accounts can register using an existing Student Number.

Faculty account creation and role assignment are managed separately from public student registration.

**Note:** Demo credentials are intended for local testing only. Do not publish real passwords, JWT signing secrets, or active authentication tokens.

## 7. Running the Application

The solution contains two projects:

- `StudentGradeManagement.API`
- `StudentGradeManagement.MVC`

Both applications must be running for the MVC frontend to communicate with the API.

### Step 1: Run the API

1. Open `StudentGradeManagementSystem.sln` in Visual Studio.
2. Set `StudentGradeManagement.API` as the startup project.
3. Run the application.

API base URL:

`http://localhost:5200`

### Step 2: Run the MVC Application

1. Open another Visual Studio instance or run the MVC project separately.
2. Set the MVC project as the startup project.
3. Run the application.

MVC base URL:

`http://localhost:5160`

### Step 3: Access the Application

Open the MVC application in your browser:

`http://localhost:5160`

Log in using a Faculty or Student account.

## 8. Grade Remark Computation

Grade remarks are computed on the server side using the application's grade remark service.

| Grade Range | Remark |
|---|---|
| 95–100 | CMS |
| 90–94 | SP |
| 85–89 | SAP |
| 80–84 | GP |
| 75–79 | EP |
| 0–74 | BEP/FAILED |

The system does not trust remarks submitted by the browser. The API calculates the correct remark based on the submitted grade.

## 9. API Endpoints

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register-student` | Register a student |
| POST | `/api/auth/login` | Authenticate a user and obtain a JWT |

### Grades

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/grades` | Retrieve all grades (Faculty) |
| GET | `/api/grades/{id}` | Retrieve a grade by ID |
| GET | `/api/grades/my-grades` | Retrieve the authenticated student's grades |
| POST | `/api/grades` | Create a grade (Faculty) |
| PUT | `/api/grades/{id}` | Update a grade (Faculty) |
| DELETE | `/api/grades/{id}` | Delete a grade (Faculty) |

All protected endpoints require valid authentication and the appropriate role.

## 10. Postman Testing

The exported Postman collection and test screenshots are located in the `Postman` folder.

The tests cover:

- Faculty retrieval of all grade records.
- Faculty creation, updating, and deletion of grades.
- Grade validation for values above 100, negative values, and decimal values.
- Server-side computation of grade remarks.
- Student access to personal grade records.
- Prevention of access to another student's grade.
- Prevention of Student accounts from creating, updating, or deleting grades.
- Unauthorized access without a JWT token.

The screenshots are organized into the following folders:

- Faculty
- Student
- Validation
- Authentication

Import `StudentGradeManagement.postman_collection.json` into Postman to access the saved API requests.

## 11. Security and Authorization

The system implements the following security measures:

- JWT-based authentication.
- Role-based authorization.
- Faculty-only grade management operations.
- Student-specific grade access using authenticated identity claims.
- Server-side validation of grade values.
- Server-side computation of grade remarks.
- Protection against unauthorized grade modification.

## 12. Project Folder Structure

```text
Fullname_WebSystemFinalProject/
│
├── StudentGradeManagement.API/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Migrations/
│   ├── Services/
│   ├── appsettings.json
│   └── Program.cs
│
├── StudentGradeManagement.MVC/
│
├── Database/
│   └── StudentGradeManagementDB_WS_FINAL_PROJECT.sql
│
├── Postman/
│   ├── StudentGradeManagement.postman_collection.json
│   ├── Faculty/
│   ├── Student/
│   ├── Validation/
│   └── Authentication/
│
├── StudentGradeManagementSystem.sln
│
└── README.md
```

## 13. Notes

- The academic database and Identity authentication database structures are managed separately.
- The academic SQL script must be executed before applying the Identity migration when setting up a fresh database.
- The API must be running before using the MVC frontend.
- Use the demo accounts only for development and testing.
