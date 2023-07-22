#!/bin/bash

# MUST BE RUN WITH . /environ... or source /environ

current_dir=$(pwd)
echo "current dir is: $current_dir"

wget https://dot.net/v1/dotnet-install.sh

chmod +x dotnet-install.sh

# dotnet
./dotnet-install.sh -c 7.0

export PATH="/home/ucloud/.dotnet:$PATH"

rm -f dotnet-install.sh

# dapr
mkdir $HOME/dapr

wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | DAPR_INSTALL_DIR="$HOME/dapr" /bin/bash -s 1.11.0

cd $HOME/dapr

./dapr init --slim --runtime-version=1.11.1

export PATH="/home/ucloud/.dapr/bin:$PATH"

export PATH="/home/ucloud/dapr:$PATH"

# dapr config
cd /work/RodrigoNunesLaigner#6308

cp DaprConfig/components/pubsub.yaml /home/ucloud/".dapr"/components
cp DaprConfig/components/statestore.yaml /home/ucloud/".dapr"/components

cp DaprConfig/config.yaml /home/ucloud/".dapr"
cp DaprConfig/resiliency.yaml /home/ucloud/".dapr"
