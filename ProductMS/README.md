dotnet ef migrations add ProductMigration -c ProductDbContext

dapr run --app-port 5008 --app-id product --app-protocol http --dapr-http-port 3508 --metrics-port 9098 -- dotnet run --project ProductMS.csproj