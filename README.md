# NovaCRM (Work in Progress)

NovaCRM is a personal full-stack CRM project built for portfolio and learning purposes.
It currently runs locally and is **not production-ready**. The database is **not hosted publicly**.

The goal of the project is to practice building a modern web app with ASP.NET Core, PostgreSQL, and React (or a front-end UI), focusing on clean backend structure, authentication, and basic CRM modules.

---

## ✅ Current Status

This project is in an early stage and actively evolving.

### Implemented (partially)
- Authentication / Authorization
- Dashboard (basic)
- Profile page (partially working)
- Clients module (partially working)
- Staff module (partially working)

### Not implemented / planned
- Public demo hosting
- Full CRUD coverage & validations
- Automated tests coverage
- Production configuration (Docker, CI/CD, cloud hosting)

---

## 🧱 Tech Stack

**Backend**
- C# / ASP.NET Core (Web API)
- Entity Framework Core
- PostgreSQL
- Swagger (OpenAPI)

**Frontend**
- React (UI in progress)

**Tools**
- Git
- Visual Studio 2022
- Postman

---

## ⚙️ Run Locally

### Prerequisites
- .NET SDK (8.0+ recommended)
- PostgreSQL
- Node.js (if running the frontend)

### 1) Backend

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
Swagger (API docs):

bash
Copy code
https://localhost:5001/swagger
2) Frontend (optional)
bash
Copy code
cd frontend
npm install
npm start
Frontend:

arduino
Copy code
http://localhost:3000
🔐 Configuration
Set your local PostgreSQL connection string in appsettings.json (or appsettings.Development.json).

Example:

json
Copy code
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=novacrm;Username=postgres;Password=yourpassword"
  }
}
Note: JWT settings / secrets should be stored securely (user-secrets or environment variables).
This repo may use simplified settings for local development.

🧭 Roadmap (High Level)
Finish Clients and Staff modules (CRUD + validation)

Improve profile workflow

Add role-based access (Admin/Staff)

Add basic unit tests (services layer)

Add Docker for local setup (optional)

Add CI pipeline (build + tests)

📌 Disclaimer
This repository is a work-in-progress portfolio project.
It is not intended for production use.

👨‍💻 Author
Rustam Urazbakhtin
GitHub: https://github.com/RustamUrazbakhtin
