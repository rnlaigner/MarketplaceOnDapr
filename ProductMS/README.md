# ProductMS

## How to run a migration
dotnet ef migrations add ProductMigration -c ProductDbContext

## How to setup the environment

# with metrics
dapr run --app-port 5008 --app-id product --app-protocol http --dapr-http-port 3508 --metrics-port 9098 -- dotnet run --project ProductMS/ProductMS.csproj

# without metrics
dapr run --app-port 5008 --app-id product --app-protocol http --dapr-http-port 3508 -- dotnet run --project ProductMS/ProductMS.csproj

