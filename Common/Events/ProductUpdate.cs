namespace Common.Events
{
    public record ProductUpdate
	(
         long seller_id,
         long product_id,
         decimal price,
         bool active,
         int instanceId
    );
}