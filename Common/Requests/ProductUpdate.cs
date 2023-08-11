namespace Common.Requests;

public record ProductUpdate(int sellerId, int productId, bool active, int instanceId);