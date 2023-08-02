dotnet ef migrations add PaymentMigration -c PaymentDbContext

dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 --metrics-port 9094 -- dotnet run --project PaymentMS.csproj