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
cd ..
current_dir=$(pwd)
echo "Starting deployment in Ubuntu..."

if `echo "$*" | grep -q cart`; then
    c=`~/dapr/dapr list | grep -c cart`
    if [ $c = $var1 ]
    then
        echo "cart already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5001 --app-id cart --app-protocol http --dapr-http-port 3501 --metrics-port 9091 -- ~/.dotnet/dotnet run --urls "http://*:5001" --project '$current_dir'/CartMS/CartMS.csproj; bash"' -T "Cart"
    fi
fi

if `echo "$*" | grep -q order`; then
    o=`~/dapr/dapr list | grep -c order`
    if [ $o = $var1 ]
    then
        echo "order already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5002 --app-id order --app-protocol http --dapr-http-port 3502 --metrics-port 9092 -- ~/.dotnet/dotnet run --urls "http://*:5002" --project '$current_dir'/OrderMS/OrderMS.csproj; bash"' -T "Order"
    fi
fi

if `echo "$*" | grep -q stock`; then
    s=`~/dapr/dapr list | grep -c stock`
    if [ $s = $var1 ]
    then
        echo "stock already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 --metrics-port 9093 -- ~/.dotnet/dotnet run --urls "http://*:5003" --project '$current_dir'/StockMS/StockMS.csproj; bash"' -T "Stock"
    fi
fi

if `echo "$*" | grep -q payment`; then
    p=`~/dapr/dapr list | grep -c payment`
    if [ $p = $var1 ]
    then
        echo "payment already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 --metrics-port 9094 -- ~/.dotnet/dotnet run --urls "http://*:5004" --project '$current_dir'/PaymentMS/PaymentMS.csproj; bash"' -T "Payment"
    fi
fi

if `echo "$*" | grep -q shipment`; then
    p=`~/dapr/dapr list | grep -c shipment`
    if [ $p = $var1 ]
    then
        echo "shipment already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5005 --app-id shipment --app-protocol http --dapr-http-port 3505 --metrics-port 9095 -- ~/.dotnet/dotnet run --urls "http://*:5005" --project '$current_dir'/ShipmentMS/ShipmentMS.csproj; bash"' -T "Shipment"
    fi
fi

if `echo "$*" | grep -q seller`; then
    p=`~/dapr/dapr list | grep -c seller`
    if [ $p = $var1 ]
    then
        echo "seller already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5006 --app-id seller --app-protocol http --dapr-http-port 3506 --metrics-port 9096 -- ~/.dotnet/dotnet run --urls "http://*:5006" --project '$current_dir'/SellerMS/SellerMS.csproj; bash"' -T "Seller"
    fi
fi

if `echo "$*" | grep -q customer`; then
    p=`~/dapr/dapr list | grep -c customer`
    if [ $p = $var1 ]
    then
        echo "customer already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5007 --app-id customer --app-protocol http --dapr-http-port 3507 --metrics-port 9097 -- ~/.dotnet/dotnet run --urls "http://*:5007" --project '$current_dir'/CustomerMS/CustomerMS.csproj; bash"' -T "Customer"
    fi
fi

if `echo "$*" | grep -q product`; then
    p=`~/dapr/dapr list | grep -c product`
    if [ $p = $var1 ]
    then
        echo "product already running"
    else
        xfce4-terminal -e 'bash -c "~/dapr/dapr run --app-port 5008 --app-id product --app-protocol http --dapr-http-port 3508 --metrics-port 9098 -- ~/.dotnet/dotnet run --urls "http://*:5008" --project '$current_dir'/ProductMS/ProductMS.csproj; bash"' -T "Product"
    fi
fi

echo "Deployment finished."