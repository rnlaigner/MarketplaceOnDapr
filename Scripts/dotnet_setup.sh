#!/bin/bash

current_dir=$(pwd)
echo "current dir is: $current_dir"

wget https://dot.net/v1/dotnet-install.sh

chmod +x dotnet-install.sh

# dotnet
./dotnet-install.sh -c 7.0

export PATH="/home/ucloud/.dotnet:$PATH"

rm -f dotnet-install.sh