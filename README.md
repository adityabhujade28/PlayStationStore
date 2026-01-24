# PSstore - Video Game Store Platform

A full-stack web application for purchasing and managing video games and subscriptions.


## Backend Setup

### Navigate to backend directory:
```bash
cd PSstore-Backend
```

### Install NuGet Packages:
```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Serilog.AspNetCore --version 10.0.0
dotnet add package Serilog.Sinks.Console --version 6.1.1
dotnet add package Serilog.Sinks.File --version 7.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
dotnet add package Bogus --version 35.5.0
```

### Configure Database:
- Open `appsettings.Development.json`
- Update the `DefaultConnection` string with your SQL Server instance

### Apply Migrations:
```bash
dotnet ef database update
```

---

## Frontend Setup

### Navigate to frontend directory:
```bash
cd PSstore-Frontend
```

### Install Dependencies:
```bash
npm install
```

### Install Additional Packages (if needed):
```bash
npm install react@19.2.0 react-dom@19.2.0
npm install react-router-dom@7.12.0
npm install tailwindcss@4.1.18 @tailwindcss/vite@4.1.18
npm install jwt-decode@4.0.0
```

---

## Running the Project

### Run Backend:
```bash
cd PSstore-Backend
dotnet run
```
### Run Frontend (in new terminal):
```bash
cd PSstore-Frontend
npm run dev
```
Frontend will be available at `http://localhost:5173`

---

