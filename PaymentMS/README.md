dotnet ef migrations add InitialMigration -c PaymentDbContext

dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 -- dotnet run --project PaymentMS.csproj