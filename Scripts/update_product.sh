#!/bin/bash

param1=1

if [ $# -eq 0 ];
then
  echo "No arguments passed. Assuming a single update product transaction."
else
  param1="$1"
fi

echo "Adding product 1/1"

echo ""

curl -X POST -H "Content-Type: application/json" -d '{"seller_id": "1", "product_id": "1", "name" : "productTest", "sku" : "skuTest", "category" : "categoryTest", "status" : "approved", "description": "descriptionTest", "price" : 10, "freight_value" : 0, "version": "1"}' localhost:5008

echo "Retrieving product 1/1"

echo ""

curl -X GET localhost:5008/1/1

echo ""

echo "Adding stock item 1/1"

echo ""

curl -X POST -H "Content-Type: application/json" -d '{"seller_id": "1", "product_id": "1", "qty_available" : 10, "qty_reserved" : 0, "order_count" : 0, "ytd": 0, "data" : "", "version": "0"}' localhost:5003

echo "Retrieving stock item 1/1"

echo ""

curl -X GET localhost:5003/1/1

echo ""

for i in `seq 1 $param1`
do

  echo "Starting iteration $i..."
  echo ""

  echo "Replacing product 1/1"

  curl -X PUT -H "Content-Type: application/json" -d '{"seller_id" : "1", "product_id" : "1", "name" : "productTest", "sku" : "skuTest", "category" : "categoryTest", "status" : "approved", "description" : "descriptionTest", "price" : 10, "freight_value" : 0, "version" : "'$i'"}' localhost:5008

  echo ""

done

echo "Update product script done"
