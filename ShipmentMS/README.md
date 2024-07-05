# ShipmentMS

## How to run a migration
dotnet ef migrations add ShipmentMigration -c ShipmentDbContext

## How to setup the environment

### with metrics
dapr run --app-port 5005 --app-id payment --app-protocol http --dapr-http-port 3505 --metrics-port 9095 -- dotnet run --project ShipmentMS.csproj

### without metrics
dapr run --app-port 5005 --app-id payment --app-protocol http --dapr-http-port 3505 -- dotnet run --project ShipmentMS.csproj

