# Food Safety Inspection Tracker

ASP.NET Core MVC application for tracking food premises inspections and follow-ups. It includes a Dashboard with counts, full CRUD for Premises, Inspections and Follow-ups, and login with roles (Admin, Inspector, Viewer).

---

## Roles & Login (Seed Users)
```
All seed users share the same password: Test@123
```
- Admin :  admin@local  Test@123  Full access: Premises, Inspections, Follow-ups (create, edit, delete).
- Inspector : inspector@local Test@123   Create/edit Inspections and Follow-ups; Premises view only.
- Viewer:  viewer@local  Test@123  Read-only (Dashboard, lists, details); no create, edit or delete
**Login URL:** **http://localhost:5034/Account/Login** — use any of the emails above with the password `Test@123`.





### 1. Prerequisites

- Install the **.NET 9 SDK**.  
  Check: run `dotnet --version` (you should see 9.x).  



### 2. Go to the Project Folder

```bash
cd FoodSafetyTracker

dotnet restore
```

### 3. Database (First Run)

- On **first run**, the app will apply migrations and create **FoodSafety.db** automatically.  
- If **FoodSafety.db** does not exist, you do not need to do anything; it will be created when you run `dotnet run`.

To apply or update migrations manually (e.g. after adding new migrations):

```bash
dotnet ef database update
```

(Requires the EF Core CLI: `dotnet tool install -g dotnet-ef`)

### 4. Run the App

```bash
dotnet run
```


### 5. Open in the Browser

- Open that URL in your browser (e.g. **http://localhost:5034**).  
- If you are not logged in, you will see the **Login** page.  
- **Email:** admin@local (or inspector@local / viewer@local)  
- **Password:** Test@123  



## Quick Run Steps

1. `cd FoodSafetyTracker`
2. `dotnet run`
3. In the browser open: **http://localhost:5034**
4. Login with **admin@local** / **Test@123** (or another role user)
5. To stop: **Ctrl+C** in the terminal

---

## Project Structure

- **FoodSafetyTracker/** — main MVC project (net9.0)
- **FoodSafetyTracker.Tests/** — xUnit test project (at least 4 tests: overdue follow-ups, closed-date validation, dashboard counts, role authorization)
- **FoodSafetyTracker/FoodSafety.db** — SQLite database (created automatically on first run)
- **FoodSafetyTracker/logs/** — Serilog rolling log files (e.g. `log-YYYYMMDD.txt`); path is relative to the app when you run `dotnet run`
- **.github/workflows/dotnet.yml** — GitHub Actions CI (build + test on push/PR)


test Ci




