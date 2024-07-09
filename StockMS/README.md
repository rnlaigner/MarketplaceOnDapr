# StockMS

## How to run a migration
dotnet ef migrations add StockMigration -c StockDbContext

## How to setup the environment

### with metrics
dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 --metrics-port 9093 -- dotnet run --urls "http://*:5003" --project StockMS/StockMS.csproj

### without metrics
dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 -- dotnet run --urls "http://*:5003" --project StockMS/StockMS.csproj

