#!/bin/bash

# Access individual command-line arguments
echo "Script name: $0"
echo "Number of arguments: $#"
echo "The arguments passed: $*"

help_="--help"
param1="$1"

if [ "$param1" = "$help_" ]; then
    echo "It is expected that the script runs in the project root folder."
    echo "You can specify the apps using the following pattern:"
    echo "<app-id1> ... <app-idn>"
    exit 1
fi

var1=1
current_dir=$(pwd)
#echo $x

if `echo "$*" | grep -q cart`; then
    c=`dapr list | grep -c cart`
    if [ $c = $var1 ]
    then
        echo "cart already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5001 --app-id cart --app-protocol http --dapr-http-port 3501 -- dotnet run --project '$current_dir'/CartMS/CartMS.csproj"
        end tell'
    fi
fi

if [ `echo "$*" | grep -q order` ]; then
    o=`dapr list | grep -c order`
    if [ $o = $var1 ]
    then
        echo "order already running"
    else
        osascript -e 'tell app "Terminal"
            do script "dapr run --app-port 5002 --app-id order --app-protocol http --dapr-http-port 3502 -- dotnet run --project '$current_dir'/OrderMS/OrderMS.csproj"
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
            do script "dapr run --app-port 5003 --app-id stock --app-protocol http --dapr-http-port 3503 -- dotnet run --project '$current_dir'/StockMS/StockMS.csproj"
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
            do script "dapr run --app-port 5004 --app-id payment --app-protocol http --dapr-http-port 3504 -- dotnet run --project '$current_dir'/PaymentMS/PaymentMS.csproj"
        end tell'
    fi
fi