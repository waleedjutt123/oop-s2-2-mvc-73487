# Food Safety Inspection Tracker (Assignment 2)

ASP.NET Core MVC application for tracking food premises inspections and follow ups, focused on Serilog logging, error handling, and dashboard reporting.

## Project structure

`FoodSafetyTracker/`  main ASP.NET Core MVC project (net9.0).

## Requirements implemented in this step

MVC project scaffolded via `dotnet new mvc`.
Serilog base packages added:
`Serilog.AspNetCore`
`Serilog.Sinks.Console`
`Serilog.Sinks.File`
`.gitignore` configured for .NET/Visual Studio build artifacts and logs.

## How to run (development)

From the `FoodSafetyTracker` folder:

dotnet ef migrations add InitialCreate
cd FoodSafetyTracker
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run




