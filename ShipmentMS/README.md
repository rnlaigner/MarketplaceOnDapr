# ShipmentMS

## How to run a migration
dotnet ef migrations add ShipmentMigration -c ShipmentDbContext

## How to setup the environment

### with metrics
dapr run --app-port 5005 --app-id payment --app-protocol http --dapr-http-port 3505 --metrics-port 9095 -- dotnet run --urls "http://*:5005" --project ShipmentMS.csproj

### without metrics

#### inside folder
dapr run --app-port 5005 --app-id payment --app-protocol http --dapr-http-port 3505 -- dotnet run --urls "http://*:5005" --project ShipmentMS.csproj

#### in root folder
dapr run --app-port 5005 --app-id payment --app-protocol http --dapr-http-port 3505 -- dotnet run --urls "http://*:5005" --project ShipmentMS/ShipmentMS.csproj
