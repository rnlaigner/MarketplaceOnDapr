#!/bin/bash

# Access individual command-line arguments
echo "Script name: $0"
echo "Number of arguments: $#"
echo "The arguments passed: $*"

help_="--help"
param1="$1"

if [ "$param1" = "$help_" ]; then
    echo "It is expected that the script runs in the project's root folder."
    echo "You can specify the apps using the following pattern:"
    echo "<app-id1> ... <app-idn>"
    exit 1
fi

var1=1
current_dir=$(pwd)
echo "Starting deployment in Ubuntu..."

if `echo "$*" | grep -q cart`; then
    c=`dapr list | grep -c cart`
    if [ $c = $var1 ]
    then
        echo "cart already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5001 --app-id cart --app-protocol http --dapr-http-port 3501 --metrics-port 9091 -- dotnet run --project '$current_dir'/CartMS/CartMS.csproj"
    fi
fi

if `echo "$*" | grep -q order`; then
    o=`dapr list | grep -c order`
    if [ $o = $var1 ]
    then
        echo "order already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5002 --app-id order --app-protocol http --dapr-http-port 3502 -- dotnet run --project '$current_dir'/OrderMS/OrderMS.csproj"
    fi
fi

if `echo "$*" | grep -q stock`; then
    s=`dapr list | grep -c stock`
    if [ $s = $var1 ]
    then
        echo "stock already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 -- dotnet run --project '$current_dir'/StockMS/StockMS.csproj"
    fi
fi

if `echo "$*" | grep -q payment`; then
    p=`dapr list | grep -c payment`
    if [ $p = $var1 ]
    then
        echo "payment already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 --metrics-port 9094 -- dotnet run --project '$current_dir'/PaymentMS/PaymentMS.csproj"
    fi
fi

if `echo "$*" | grep -q shipment`; then
    p=`dapr list | grep -c shipment`
    if [ $p = $var1 ]
    then
        echo "shipment already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5005 --app-id shipment --app-protocol http --dapr-http-port 3505 -- dotnet run --project '$current_dir'/ShipmentMS/ShipmentMS.csproj"
    fi
fi

if `echo "$*" | grep -q seller`; then
    p=`dapr list | grep -c seller`
    if [ $p = $var1 ]
    then
        echo "seller already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5006 --app-id seller --app-protocol http --dapr-http-port 3506 -- dotnet run --project '$current_dir'/SellerMS/SellerMS.csproj"
    fi
fi

if `echo "$*" | grep -q customer`; then
    p=`dapr list | grep -c customer`
    if [ $p = $var1 ]
    then
        echo "customer already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5007 --app-id customer --app-protocol http --dapr-http-port 3507 -- dotnet run --project '$current_dir'/CustomerMS/CustomerMS.csproj"
    fi
fi

if `echo "$*" | grep -q product`; then
    p=`dapr list | grep -c product`
    if [ $p = $var1 ]
    then
        echo "product already running"
    else
        xterm -e "~/dapr/dapr run --app-port 5008 --app-id product --app-protocol http --dapr-http-port 3508 -- dotnet run --project '$current_dir'/ProductMS/ProductMS.csproj"
    fi
fi