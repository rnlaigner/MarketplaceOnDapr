dotnet ef migrations add SellerMigration -c SellerDbContext

dapr run --app-port 5006 --app-id payment --app-protocol http --dapr-http-port 3506 -- dotnet run --project SellerMS.csproj

In a marketplace it is usually the case sellers have a kind of dashboard (usually after login) to get info about their sales.
For instance, a dashboard presents important information about the operation of a given seller, such as the revenue aggregated
by time, top-10 items with less number of stock, the most popular products, the open orders, etc

Seller views:

OrderSellerView -> Orders in progress...
INVOICED and IN_TRANSIT

OrderEntries -> Packages in progress...
created, shipped

ProductView -> most popular products... can take items from here to update price...
Products that may need replenishment

Historical:
Overall historical orders
Overall historical shipments

Two approaches for maintaining the seller views:

1. calculating average in memory (customized code) and saving in the db. pushing count to the database
    we can do fully optimized code, that is not what we are competing with
    the way people are doing is the way it should be (doing database tasks at the app layer)?
    it is because the DB does not support them effectively?
2. storing all tuples in seller DB and pushing the aggregates to the DBMS
3. using a continuous query, external system, streaming database.

For a fair comparison, a better approach is going for materialized views


Correctness criteria (to discuss

I believe an order cannot be included in the revenue (provided by the seller ms) if the HTTP response
from order says the order is still open (even though it has been finished on shipment and already processed
in the seller microservice. i.e., the order finish event has not yet arrived in the order ms). The same can
be applied to the most popular products... )

A consistent/transactional cut (or snapshot). No “effect” event can be exposed without the “cause”.
Example, although a shipment may have finished (which naturally makes it unfit for the shipment drilldown),
it is possible that the respective order has not been included in the order drilldown yet (e.g., late event).

The event processing does not safeguard ordering.