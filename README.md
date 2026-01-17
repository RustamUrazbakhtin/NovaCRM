# NovaCRM

NovaCRM is a web-based CRM system designed for small businesses, with an initial focus on beauty salons and service-based companies.

The project is built as a full-stack application using ASP.NET Core, PostgreSQL, and React, with a strong focus on clean backend architecture, scalability, and maintainability.

---

## 🚀 Features

- User authentication and authorization (JWT)
- Role-based access control (Admin / Manager / Staff)
- Client management (CRUD)
- Appointment and schedule management
- Services catalog
- Basic analytics and dashboard
- REST API architecture
- Database migrations with Entity Framework Core

---

## 🧱 Architecture

**Backend**
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Layered architecture (Controllers, Services, Repositories)
- DTOs and validation

**Frontend**
- React
- REST API integration
- Modular UI structure

**Authentication**
- JWT access tokens
- Role-based authorization

---

## 🛠 Tech Stack

**Backend**
- C#
- .NET / ASP.NET Core
- Entity Framework Core
- PostgreSQL
- REST APIs
- Swagger

**Frontend**
- React
- JavaScript / TypeScript

**Tools**
- Git
- Visual Studio 2022
- Postman

---

## ⚙️ Getting Started

### Prerequisites

- .NET SDK 8.0+
- PostgreSQL
- Node.js (for frontend)

---

### Backend setup

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
