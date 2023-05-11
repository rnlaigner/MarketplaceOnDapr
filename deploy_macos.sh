#!/bin/bash
x=`dapr list | grep -c cart`
y=`dapr list | grep -c order`
z=`dapr list | grep -c stock`
var1=1

#echo $x

if [ $x = $var1 ]
then
    echo "cart already running"
else
    osascript -e 'tell app "Terminal"
        do script "dapr run --app-port 5001 --app-id cart --app-protocol http --dapr-http-port 3501 -- dotnet run --project ~/workspace/benchmark/MarketplaceDapr/CartMS/CartMS.csproj"
    end tell'
fi

if [ $y = $var1 ]
then
    echo "order already running"
else
    osascript -e 'tell app "Terminal"
        do script "dapr run --app-port 5002 --app-id order --app-protocol http --dapr-http-port 3502 -- dotnet run --project ~/workspace/benchmark/MarketplaceDapr/OrderMS/OrderMS.csproj"
    end tell'
fi

if [ $z = $var1 ]
then
    echo "stock already running"
else
    osascript -e 'tell app "Terminal"
        do script "dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 -- dotnet run --project ~/workspace/benchmark/MarketplaceDapr/StockMS/StockMS.csproj"
    end tell'
fi