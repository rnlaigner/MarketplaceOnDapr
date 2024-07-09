# PaymentMS

## How to run a migration
dotnet ef migrations add PaymentMigration -c PaymentDbContext

## with metrics
dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 --metrics-port 9094 -- dotnet run --urls "http://*:5004" --project PaymentMS.csproj

## inside the PaymentMS folder
dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 -- dotnet run --urls "http://*:5004" --project PaymentMS.csproj

## in the root folder
dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 -- dotnet run --urls "http://*:5004" --project PaymentMS/PaymentMS.csproj

