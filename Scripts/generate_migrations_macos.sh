#!/bin/bash

echo "Script name: $0"

help_="--help"
param1="$1"

if [ "$param1" = "$help_" ]; then
    echo "It is expected that the script runs in the project's root folder."
    exit 1
fi

var1=1
current_dir=$(pwd)

osascript -e 'tell app "Terminal"
    do script "cd '$current_dir'/OrderMS && dotnet ef migrations add InitialMigration -c OrderDbContext"
end tell'

osascript -e 'tell app "Terminal"
    do script "cd '$current_dir'/PaymentMS && dotnet ef migrations add InitialMigration -c PaymentDbContext"
end tell'

osascript -e 'tell app "Terminal"
    do script "cd '$current_dir'/SellerMS && dotnet ef migrations add InitialMigration -c SellerDbContext"
end tell'

osascript -e 'tell app "Terminal"
    do script "cd '$current_dir'/ShipmentMS && dotnet ef migrations add InitialMigration -c ShipmentDbContext"
end tell'

osascript -e 'tell app "Terminal"
    do script "cd '$current_dir'/StockMS && dotnet ef migrations add InitialMigration -c StockDbContext"
end tell'