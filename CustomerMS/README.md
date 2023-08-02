dotnet ef migrations add CustomerMigration -c CustomerDbContext

dapr run --app-port 5007 --app-id customer --app-protocol http --dapr-http-port 3507 -- dotnet run --project CustomerMS.csproj