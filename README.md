# AssetCore – Enterprise IT Asset & Maintenance Management System

> **Modular Hardware Custody Tracking · Lifecycle Maintenance · Role-Based Administration**
>
> Built with ASP.NET Core (.NET 10) · Entity Framework Core · SQL Server · Bootstrap 5 · C# 12

![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![C%23](https://img.shields.io/badge/C%23-12.0-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-indigo)
![Entity Framework Core](https://img.shields.io/badge/ORM-EF%20Core-512BD4)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple)
![Licence](https://img.shields.io/badge/Licence-MIT-yellow)

---

## Table of Contents

1. [Project Description](#1-project-description)
2. [System Architecture & Design](#2-system-architecture--design)
3. [Technologies Used](#3-technologies-used)
4. [Installation Instructions](#4-installation-instructions)
5. [Environment Variables & Configuration](#5-environment-variables--configuration)
6. [Usage Instructions](#6-usage-instructions)
7. [API & Controller Endpoints](#7-api--controller-endpoints)
8. [Project Structure](#8-project-structure)
9. [Features & Role Permissions Matrix](#9-features--role-permissions-matrix)
10. [Contact Information](#10-contact-information)

---

## 1. Project Description

### Overview

**AssetCore** is an enterprise-grade hardware and equipment lifecycle governance platform. It provides corporate IT departments, organizational custodians, and administrative staff with centralized tracking for physical equipment custody, departmental allocations, internal equipment requests, and complete maintenance lifecycles.

### Problem Statement

Medium-to-large business environments face recurring operational challenges with physical equipment:

* **Hardware Loss & Ghost Assets:** Unrecorded handoffs and undocumented physical transfers lead to lost equipment during employee offboarding.
* **Uncoordinated Maintenance:** Lack of service ticket tracking causes untracked equipment repair costs and unclear machine readiness.
* **Uncontrolled Account Elevation:** Standard systems often rely on bulky third-party identity frameworks that obscure custom domain roles and account lockout policies.
* **Disjointed Equipment Identification:** Displaying vague equipment descriptions without explicit Serial Number pairing causes technician errors during assignments and field service repairs.

### Objectives

- **Precise Hardware Identification:** Enforce paired `[Serial Number] - [Asset Name]` selectors across all workflows to eliminate servicing and assignment mistakes.
- **Role-Gated Operations:** Provide distinct interfaces and authorization boundaries for `Admin`, `ITTechnician`, `OfficeUser`, and `Auditor` personas.
- **Structured Maintenance Lifecycles:** Track hardware repair tickets from initial scheduling through vendor handling, cost calculation, and final sign-off.
- **Standardized Visual Ergonomics:** Deliver a unified design system using Bootstrap 5 centered form cards, card-grid directories, and real-time DOM filtering.

---

## 2. System Architecture & Design

### Application Topology

The application uses an N-Tier Model-View-Controller (MVC) architecture with customized Cookie-based claim security, decoupled relational entities, and client-side document filtering.

```
+---------------------+        +----------------------+        +---------------------+
|      Frontend       |        |       Backend        |        |      Database       |
|  Razor Views (MVC)  | -----> | ASP.NET Core (.NET10) | -----> |  Microsoft          |
|  Bootstrap 5.3      | <----- | Controllers, Custom   | <----- |  SQL Server         |
|  Vanilla JS Filter  |        | Claims Auth, LINQ     |        |  (AssetsEmployeeDb) |
|  Port 5000 / 5001   |        | EF Core Code-First    |        |                     |
+---------------------+        +----------------------+        +---------------------+
```

### Core Data Models

| Entity Model | Primary Key | Key Attributes | Relationships |
|---|---|---|---|
| `User` | `UserId` | `Username`, `PasswordHash`, `Role`, `LockoutEnd`, `AccessFailedCount` | Independent access management |
| `Department` | `DepartmentId` | `DepartmentName`, `Location`, `Description` | 1-to-Many with `Employee` |
| `Employee` | `EmployeeId` | `EmployeeName`, `Email`, `PhoneNo` | Belongs to `Department`, 1-to-Many with `EmployeeAsset` |
| `Asset` | `AssetId` | `AssetName`, `SerialNo`, `Model`, `Status`, `PurchaseDate` | 1-to-Many with `EmployeeAsset` & `MaintenanceRecord` |
| `EmployeeAsset` | `Id` | `AssignedDate` | Junction table binding `Employee` and `Asset` |
| `MaintenanceRecord` | `MaintenanceId` | `Title`, `Description`, `ServiceVendor`, `Cost`, `Status`, `ScheduledDate`, `CompletedDate`, `LoggedBy` | Belongs to `Asset` |
| `AppNotification` | `Id` | `Message`, `TargetRole`, `IsCleared`, `CreatedAt` | Role-targeted administrative alert feed |

### Data Flow

Browser (User)                 EmployeeAssetController          Entity Framework Core            SQL Server
     │                                    │                               │                          │
  1  ├── Open Assign Asset View ─────────►│                               │                          │
     │   GET /EmployeeAsset/Assign        ├── Fetch Available Assets ────►│                          │
     │                                    │   (Status == 'Available')     ├── SELECT AssetId, SN, ──►│
     │                                    │   & Active Employees          │   AssetName FROM Asset   │
     │                                    │                               ├── SELECT EmployeeId, ───►│
     │                                    │                               │   EmployeeName...        │
     │◄── Render Assignment Form Card ────┴───────────────────────────────┤                          │
     │    (Enforces [SN] - [Asset Name])                                                             │
     │                                                                                               │
  2  ├── Submit Assignment Form ─────────►│                               │                          │
     │   POST /EmployeeAsset/Assign       ├── Validate Asset & Employee   │                          │
     │   { AssetId, EmployeeId }          ├── Add EmployeeAsset Link ────►│                          │
     │                                    ├── Update Asset Status:        │                          │
     │                                    │   'Available' -> 'Assigned'   ├── BEGIN TRANSACTION      │
     │                                    │                               ├── INSERT INTO            │
     │                                    │                               │   EmployeeAssets... ────►│
     │                                    │                               ├── UPDATE Assets          │
     │                                    │                               │   SET Status='Assigned'─►│
     │                                    │                               ├── COMMIT TRANSACTION ───►│
     │◄── Redirect to Custody Ledger ─────┴───────────────────────────────┤                          │
     │                                                                                               │
  3  ├── Revoke Custody (Return Asset) ──►│                               │                          │
     │   POST /EmployeeAsset/Delete/{id}  ├── Remove EmployeeAsset Link ─►│                          │
     │                                    ├── Restore Asset Status:       ├── BEGIN TRANSACTION      │
     │                                    │   'Assigned' -> 'Available'   ├── DELETE FROM            │
     │                                    │                               │   EmployeeAssets... ────►│
     │                                    │                               ├── UPDATE Assets          │
     │                                    │                               │   SET Status='Available'►│
     │                                    │                               ├── COMMIT TRANSACTION ───►│
     │◄── Refresh Active Custody Ledger ──┴───────────────────────────────┤                          │
     │                                                                                               │
  4  ├── Instant Custody Filter ──────────┼───────────────────────────────┼──────────────────────────┤
     │   JS Evaluates Custody Cards       │ (Client-side execution only   │                          │
     │   Matches SN, Employee, or Dept    │  zero server roundtrip delay) │                          │

## 3. Technologies Used

| Category | Technology | Version | Purpose |
|---|---|---|---|
| Web Framework | ASP.NET Core MVC | .NET 10.0 | High-performance enterprise web engine |
| Language | C# | 12.0 | Application controllers, entity definitions, and logic |
| ORM | Entity Framework Core | 10.0+ | Code-First schema management and relational LINQ queries |
| Database | Microsoft SQL Server | 2019+ | Relational persistence, constraints, foreign key cascades |
| UI Framework | Bootstrap | 5.3+ | Centered form cards, responsive card-grid layouts |
| Iconography | Bootstrap Icons | 1.11+ | UI navigation and contextual status icons |
| Client Scripting | JavaScript (ES6+) | Vanilla | Real-time DOM search filtering on card grids |
| Security | Custom Claims Cookie Auth | Core Security | Multi-role access barriers and lockout management |

---

## 4. Installation Instructions

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/) (LocalDB, Express, or Developer Edition)
- Visual Studio 2022 / VS Code with C# Dev Kit
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/NimshanDilunika/AssetCoreManagementSystem.git
cd AssetCoreManagementSystem
```

### 2. Configure Database Connection

Open `appsettings.json` and set your local SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AssetCoreDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Run Database Migrations

Apply the Entity Framework Core migrations to construct your database schema:

```bash
dotnet ef database update
```

Alternatively, execute migrations directly inside the Visual Studio Package Manager Console:

```powershell
Update-Database
```

### 4. Run the Application

```bash
dotnet run
```

Access the system in your browser:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

---

## 5. Environment Variables & Configuration

Key settings configured in `appsettings.json`:

| Key | Description | Example |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | `Server=localhost;Database=AssetCoreDb;Trusted_Connection=True;TrustServerCertificate=True;` |
| `Logging:LogLevel:Default` | Core log output verbosity | `Information` |

---

## 6. Usage Instructions

**Step 1 — Start the Application**

```bash
dotnet run
```

**Step 2 — Log In with Role Credentials**

Navigate to `https://localhost:5001/Account/Login` and sign in using your provisioned user account.

**Step 3 — Navigate Workflows**

| Action | How |
|---|---|
| Manage Users | Navigate to Users → Create, update roles, unlock accounts, or run confirmed deletion |
| Register Hardware | Go to Assets → Register asset name, model, serial number, and status |
| Track Custody | Navigate to Asset Assignment → Link unassigned hardware to an employee |
| Schedule Repair | Go to Maintenance → Log Maintenance → Pick [Serial No] - [Asset Name] |
| Track Service Costs | Update ticket status (Scheduled → InProgress → Completed) and enter finalized repair costs |
| Search Catalogs | Enter serial numbers, model names, or vendor keywords into the real-time card search bar |

---

## 7. API & Controller Endpoints

Base URL: `https://localhost:5001`

| Method | Endpoint | Description | Role Access |
|---|---|---|---|
| GET | `/Account/Login` | Login portal | Anonymous |
| POST | `/Account/Login` | Authenticate credentials and establish session | Anonymous |
| GET | `/Users` | User management index directory | Admin |
| POST | `/Users/Create` | Provision new system login account | Admin |
| GET | `/Asset` | Card-grid inventory of hardware assets | Admin, ITTechnician, Auditor |
| POST | `/Asset/Create` | Register new physical asset | Admin, ITTechnician |
| GET | `/EmployeeAsset` | Custody assignment registry | Admin, ITTechnician, Auditor |
| POST | `/EmployeeAsset/Assign` | Assign asset to an employee profile | Admin, ITTechnician |
| GET | `/Maintenance` | Maintenance card-grid directory | Admin, ITTechnician, Auditor |
| POST | `/Maintenance/AddMaintenance` | Schedule service ticket with vendor & estimated cost | Admin, ITTechnician |
| POST | `/Maintenance/UpdateMaintenance` | Update service progress, completion date, and cost | Admin, ITTechnician |
| POST | `/Maintenance/DeleteMaintenance` | Execute confirmed deletion of service ticket | Admin, ITTechnician |

---

## 8. Project Structure

```
AssetsEmployee/
│
├── Controllers/
│   ├── AccountController.cs            # Custom claim login, authentication, and sign-out
│   ├── AssetController.cs              # Asset registration, specification tracking, and card index
│   ├── DepartmentController.cs         # Department divisions, locations, and personnel groupings
│   ├── EmployeeAssetController.cs      # Custody assignment, returns, and transfer operations
│   ├── EmployeeController.cs           # Employee records, regex-validated contact details
│   ├── MaintenanceController.cs        # Module 4 service tickets, vendor costs, and status tracking
│   └── UserController.cs               # Admin user provisioning, credential changes, and lockouts
│
├── Models/
│   ├── ApplicationDbContext.cs         # EF Core relational context and foreign key configurations
│   ├── AppNotification.cs              # Role-targeted alerts and persistent navbar notifications
│   ├── Asset.cs                        # Physical hardware entity
│   ├── Department.cs                   # Corporate department division entity
│   ├── Employee.cs                     # Corporate employee profile entity
│   ├── EmployeeAsset.cs                # Custody linking entity
│   ├── MaintenanceRecord.cs            # Maintenance ticket, repair cost, and vendor entity
│   └── User.cs                         # System credentials and security profile entity
│
├── Views/
│   ├── Account/
│   │   └── Login.cshtml                # Centered login form card
│   ├── Asset/
│   │   ├── Create.cshtml               # Hardware registration view
│   │   └── Index.cshtml                # Card-grid hardware catalog with instant search
│   ├── EmployeeAsset/
│   │   ├── Assign.cshtml               # Equipment assignment form
│   │   └── Index.cshtml                # Custody ledger and equipment return tracking
│   ├── Maintenance/
│   │   ├── AddMaintenance.cshtml       # Service ticket registration form
│   │   ├── ConfirmDeleteMaintenance.cshtml # Danger-styled confirmed deletion card
│   │   ├── Index.cshtml                # Maintenance card-grid with serial search & status badges
│   │   └── UpdateMaintenance.cshtml    # Service progress and cost update form
│   ├── Shared/
│   │   └── _Layout.cshtml              # Fixed sticky navbar, role-gated menus, alert bell
│   └── User/
│       ├── Create.cshtml               # Centered account creation form
│       └── Index.cshtml                # User account administration directory
│
├── wwwroot/                            # Static CSS, JavaScript libraries, icons
│   ├── css/
│   └── js/
├── appsettings.json                    # Configuration, SQL connection strings
├── appsettings.Development.json        # Local development configurations
├── Program.cs                          # Application startup, DI pipeline, authentication config
└── AssetsEmployee.sln                  # Visual Studio Solution file
```

---

## 9. Features & Role Permissions Matrix

### System Features

- **Serialized Asset Pairing:** Selectors and detail views combine `[Serial No] - [Asset Name]` to prevent operational errors during hardware assignments and servicing.
- **Card-Grid Presentation:** Replaces static table views with responsive card layouts featuring contextual badge indicators (Available, Scheduled, InProgress, Completed).
- **Instant Client-Side Filtering:** JavaScript-powered search filters cards across titles, serial numbers, vendors, and status badges with zero page reloads.
- **Confirmed Deletion Architecture:** Destructive operations utilize dedicated confirmation views with danger-themed cards and contextual record summaries to prevent accidental data loss.
- **Persistent Sticky Header:** Unified desktop navigation with role-aware action links and active session details.

### Role Permissions Matrix

| Functional Action | Admin | IT Technician | Office User | Auditor |
|---|---|---|---|---|
| Manage Users, Roles & Lockouts | Full Access | No | No | No |
| Manage Departments & Employees | Full Access | View Only | No | View Only |
| Register & Edit Assets | Full Access | Full Access | View Only | View Only |
| Assign & Revoke Asset Custody | Full Access | Full Access | No | View Only |
| Schedule & Update Maintenance | Full Access | Full Access | No | View Only |
| Delete Maintenance Tickets | Full Access | Full Access | No | No |
| Submit Hardware Requests | Full Access | Full Access | Create/View Own | No |
| View Notifications Center | Admin Feed | Tech Feed | User Feed | No |

---

## 10. Contact Information

| Name | Email | Role / Affiliation |
|---|---|---|
| T.P.D. Nimshan Tharamasinghe | nimshandilunika@gmail.com | Lead Developer |

## Licence

This project is licensed under the MIT Licence.

```
MIT License

Copyright (c) 2026 AssetCore Management System

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

