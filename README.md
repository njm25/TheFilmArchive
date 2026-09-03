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
	"Site": {
	  "BaseUrl": "http://localhost:4200"
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

  * Ask for this value, or request one from TMDB
* **Discord.Token**

  * Generated from Discord Developer Portal
* **Discord.UserId**

  * Your Discord user ID (Developer Mode required)
* **Site.BaseUrl**

  * Public origin the frontend is served from, used to build canonical URLs for link previews
  * `http://localhost:4200` locally, `https://thefilmarchive.org` in production
  * Optional `Site.ShellUrl` overrides where the API fetches `index.html` from (defaults to `{BaseUrl}/index.html`)

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

## Link Previews (Open Graph)

Sharing a film link in Discord, iMessage, Slack, etc. renders a rich card. Social
crawlers don't run JavaScript, so the tags can't be set by the Angular app at
runtime - they have to be in the HTML response.

`GET /embed/film/{id}` on the API returns the deployed `index.html` with the Open
Graph tags for that film injected into its `<head>`. Crawlers read the tags;
browsers boot the app as normal. If the shell can't be fetched, the endpoint
falls back to a self-contained card that still carries the tags.

Amplify Hosting rewrite rules can only branch on country, not user agent, so
`/film/*` is rewritten for every visitor rather than for crawlers only.

### Amplify rewrite rule

In the Amplify console under **Hosting → Rewrites and redirects**, add this
**above** the default SPA rule (rules are applied top-down, and the SPA catch-all
would otherwise swallow it):

| Source          | Target                                              | Type           |
| --------------- | --------------------------------------------------- | -------------- |
| `/film/<*>`     | `https://api.thefilmarchive.org/embed/film/<*>`      | 200 (Rewrite)  |

Consequences worth knowing:

* Film pages are served through the API rather than the CDN, so they depend on
  the backend being up.
* The API caches `index.html` for 5 minutes, so a frontend deploy can take that
  long to show up on `/film/*`.

To verify a change, paste a film URL into an Open Graph debugger such as
[opengraph.xyz](https://www.opengraph.xyz). Discord caches unfurls per URL for
hours - append a throwaway query string (`?1`) to force a fresh fetch.
