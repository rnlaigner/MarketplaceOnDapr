#!/bin/bash

mkdir $HOME/dapr

wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | DAPR_INSTALL_DIR="$HOME/dapr" /bin/bash -s 1.11.0

cd $HOME/dapr

./dapr init --slim --runtime-version=1.11.1

export PATH="/home/ucloud/.dapr/bin:$PATH"

export PATH="/home/ucloud/dapr:$PATH"

# dapr config
cd /work/Home/EventBenchmark

cp DaprConfig/components/pubsub.yaml /home/ucloud/".dapr"/components
cp DaprConfig/components_in_memory/statestore.yaml /home/ucloud/".dapr"/components

cp DaprConfig/config.yaml /home/ucloud/".dapr"
cp DaprConfig/resiliency.yaml /home/ucloud/".dapr"
