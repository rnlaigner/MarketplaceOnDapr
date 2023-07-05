dotnet ef migrations add InitialMigration -c StockDbContext

dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 --metrics-port 9093 -- dotnet run --project StockMS.csproj