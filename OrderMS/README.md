# OrderMS

## How to setup migration

https://github.com/dotnet/sdk/issues/27761
To install ef core tools in mac x64:
dotnet tool install --global --arch x64 dotnet-ef

Managing migrations:
https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

To build the first version:
dotnet ef migrations add OrderMigration -c OrderDbContext

if needs to delete the db file and recreate it:
dotnet ef database update

## How to setup PostgreSQL

docker pull postgres

docker run --name orderdb -p 5432:5432 -e POSTGRES_PASSWORD=password -d postgres

## How to setup the environment

### without metrics

#### run in the OrderMS folder
dapr run --app-id order --app-port 5002 --app-protocol http --dapr-http-port 3502 -- dotnet run --urls "http://*:5002" --project OrderMS.csproj

#### run in root folder
dapr run --app-id order --app-port 5002 --app-protocol http --dapr-http-port 3502 -- dotnet run --urls "http://*:5002" --project OrderMS/OrderMS.csproj