#!/bin/bash

# Access individual command-line arguments
echo "Script name: $0"
echo "Number of arguments: $#"
echo "The arguments passed: $*"

help_="--help"
param1="$1"

if [ "$param1" = "$help_" ]; then
    echo "You can specify the apps using the following pattern:"
    echo "<app-id1> ... <app-idn>"
    exit 1
fi

var1=1
cd ..
current_dir=$(pwd)
echo "Running dir is" $current_dir

if `echo "$*" | grep -q cart`; then
    c=`dapr list | grep -c cart`
    if [ $c = $var1 ]
    then
        echo "cart already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5001 --app-id cart --app-protocol http --dapr-http-port 3501 -- dotnet run --urls '"'http://*:5001'"' --project '$current_dir'/CartMS/CartMS.csproj"
        end tell'
    fi
fi

if `echo "$*" | grep -q order`; then
    o=`dapr list | grep -c order`
    if [ $o = $var1 ]
    then
        echo "order already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5002 --app-id order --app-protocol http --dapr-http-port 3502 -- dotnet run --urls '"'http://*:5002'"' --project '$current_dir'/OrderMS/OrderMS.csproj"
        end tell'
    fi
fi

if `echo "$*" | grep -q stock`; then
    s=`dapr list | grep -c stock`
    if [ $s = $var1 ]
    then
        echo "stock already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 -- dotnet run --urls '"'http://*:5003'"' --project '$current_dir'/StockMS/StockMS.csproj"
        end tell'
    fi
fi

if `echo "$*" | grep -q payment`; then
    p=`dapr list | grep -c payment`
    if [ $p = $var1 ]
    then
        echo "payment already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 -- dotnet run --urls '"'http://*:5004'"' --project '$current_dir'/PaymentMS/PaymentMS.csproj"
        end tell'
    fi
fi

if `echo "$*" | grep -q shipment`; then
    p=`dapr list | grep -c shipment`
    if [ $p = $var1 ]
    then
        echo "shipment already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5005 --app-id shipment --app-protocol http --dapr-http-port 3505 -- dotnet run --urls '"'http://*:5005'"' --project '$current_dir'/ShipmentMS/ShipmentMS.csproj"
        end tell'
    fi
fi

if `echo "$*" | grep -q seller`; then
    p=`dapr list | grep -c seller`
    if [ $p = $var1 ]
    then
        echo "seller already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5006 --app-id seller --app-protocol http --dapr-http-port 3506 -- dotnet run --urls '"'http://*:5006'"' --project '$current_dir'/SellerMS/SellerMS.csproj"
        end tell'
    fi
fi

if `echo "$*" | grep -q customer`; then
    p=`dapr list | grep -c customer`
    if [ $p = $var1 ]
    then
        echo "customer already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5007 --app-id customer --app-protocol http --dapr-http-port 3507 -- dotnet run --urls '"'http://*:5007'"' --project '$current_dir'/CustomerMS/CustomerMS.csproj"
        end tell'
    fi
fi

if `echo "$*" | grep -q product`; then
    p=`dapr list | grep -c product`
    if [ $p = $var1 ]
    then
        echo "product already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5008 --app-id product --app-protocol http --dapr-http-port 3508 -- dotnet run --urls '"'http://*:5008'"' --project '$current_dir'/ProductMS/ProductMS.csproj"
        end tell'
    fi
fi
