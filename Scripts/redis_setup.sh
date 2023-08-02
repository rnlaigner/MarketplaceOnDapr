#!/bin/bash

cd $HOME

# redis: install redis from source
wget https://download.redis.io/redis-stable.tar.gz

tar -xzvf redis-stable.tar.gz

cd redis-stable

make

cd src

./redis-server --save "" --protected-mode no --io-threads 12 --io-threads-do-reads yes

# set bind to 0.0.0.0
# https://stackoverflow.com/questions/19091087/open-redis-port-for-remote-connections