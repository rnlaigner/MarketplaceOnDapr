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
