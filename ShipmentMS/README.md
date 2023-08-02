dotnet ef migrations add ShipmentMigration -c ShipmentDbContext

dapr run --app-port 5005 --app-id payment --app-protocol http --dapr-http-port 3505 -- dotnet run --project ShipmentMS.csproj
