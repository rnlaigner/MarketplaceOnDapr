# PaymentMS README

dotnet ef migrations add PaymentMigration -c PaymentDbContext

## with metrics
dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 --metrics-port 9094 -- dotnet run --project PaymentMS.csproj

## inside the PaymentMS folder
dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 -- dotnet run --project PaymentMS.csproj

## in the root folder
dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 -- dotnet run --project PaymentMS/PaymentMS.csproj

