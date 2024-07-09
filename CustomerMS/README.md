# CustomerMS

## How to run a migration
dotnet ef migrations add CustomerMigration -c CustomerDbContext

## How to setup the environment

### with metrics
dapr run --app-port 5007 --app-id customer --app-protocol http --dapr-http-port 3507 --metrics-port 9097 -- dotnet run --urls "http://*:5007" --project CustomerMS.csproj

### without metrics
dapr run --app-port 5007 --app-id customer --app-protocol http --dapr-http-port 3507 -- dotnet run --urls "http://*:5007" --project CustomerMS.csproj
