# customer adopt the dapr state management
# dotnet ef migrations add InitialMigration -c CustomerDbContext

dapr run --app-port 5007 --app-id customer --app-protocol http --dapr-http-port 3507 -- dotnet run --project CustomerMS.csproj