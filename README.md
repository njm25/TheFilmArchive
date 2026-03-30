## Overview

This project consists of:

* **Backend:** .NET
* **Frontend:** Angular
* **Database:** Microsoft SQL Server

## Prerequisites

Ensure the following are installed:

* Visual Studio 2026 (with .NET workload)
* .NET 10 SDK
* Node.js (LTS recommended)
* Docker (for SQL Server)

## Required Configuration

Add the following to your backend `appsettings.Development.json` (or equivalent):

```json 
{
	"ConnectionStrings": {
	  "DefaultConnection": ""
	},
	"Tmdb": {
	  "ApiKey": ""
	},
	"Discord": {
	  "Token": "",
	  "UserId": 0
	},
	"Jwt": {
	  "Issuer": "TheFilmArchive",
	  "Audience": "TheFilmArchiveUsers",
	  "Key": "dev-only-secret-key-change-me"
	}
}
```

### Values

* **DefaultConnection**

  * Connection string to your local SQL Server instance
* **Tmdb.ApiKey**

  * Ask for this value
* **Discord.Token**

  * Generated from Discord Developer Portal
* **Discord.UserId**

  * Your Discord user ID (Developer Mode required)
* **Jwt.Issuer**

  * No change needed
* **Jwt.Audience**

  * No change needed

* **Jwt.Key**
  * Secret used to sign JWT tokens
  * For development, any sufficiently long string is acceptable

## Local Setup Instructions

### 1. Database Setup (SQL Server via Docker)

Run a SQL Server container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrongPassword123!" \
-p 1433:1433 --name sqlserver \
-d mcr.microsoft.com/mssql/server:2022-latest
```

Verify it's running, then connect using SSMS:

* Server: `localhost,1433`
* Login: `sa`
* Password: (same as above)

Use this for your connection string:

```
Server=localhost,1433;Database=TheFilmArchive;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;
```

### 2. Backend Setup (.NET)

Open the `.sln` file in **Visual Studio 2026**

Then:

1. Restore/build (usually automatic, otherwise run manually):

```powershell
dotnet clean
dotnet build
```

2. Ensure:

   * SQL Server is running
   * Connection string is valid

3. Run the project:

   * Press **Start / Play** in Visual Studio

Notes:

* For development, you can run migrations manually in the Infrastructure project:

```powershell
update-database
```

### 3. Frontend Setup (Angular)

Open:

```
apps/client
```

in **VS Code**

Then:

```bash
npm install
npm start
```

This starts the Angular dev server.

### 4. Discord Bot Setup 

1. Go to Discord Developer Portal

2. Create a new application

3. Navigate to **Bot tab**

   * Add a bot
   * Click **Reset Token**
   * Copy token → use in config

4. Go to **OAuth2 → URL Generator**

   * Scopes: `bot`
   * Permissions: `Administrator`
   * Open generated URL → add bot to server

5. Enable Developer Mode in Discord:

   * Settings → Advanced → Developer Mode

6. Get your user ID:

   * Right-click profile → Copy ID
   * Add to config

## Development Workflow

Preferred tooling:

* **Backend:** Visual Studio 2026
* **Frontend:** VS Code (`apps/client`)
* **Database:** SSMS

Typical startup sequence:

1. Start SQL Server (Docker)
2. Run backend (Visual Studio)
3. Run frontend (`npm start`)

## Notes

* Do not commit secrets (API keys, tokens, connection strings)

## Missing Configuration

* TMDb API key is required, request separately
* Connection string depends on local SQL setup
