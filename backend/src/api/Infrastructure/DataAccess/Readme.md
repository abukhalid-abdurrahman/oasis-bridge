enter to this path : TeamPulse\src\backend
command for migrations: dotnet ef migrations add "name" -p .\Infrastructure\ -s .\WebAPI\ --output-dir DataAccess/Migrations
command for database update : dotnet ef database update -p .\Infrastructure\ -s .\WebAPI\ 