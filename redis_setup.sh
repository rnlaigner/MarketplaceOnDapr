#!/bin/bash

cd $HOME

# redis: install redis from source
wget https://download.redis.io/redis-stable.tar.gz

tar -xzvf redis-stable.tar.gz

cd redis-stable

make

cd src

./redis-server '--protected-mode no'

# set bind to 0.0.0.0
# https://stackoverflow.com/questions/19091087/open-redis-port-for-remote-connections