namespace Common.Events;

public record PriceUpdated
(
    int seller_id,
    int product_id,
    float price,
    int version,
    int instanceId
);