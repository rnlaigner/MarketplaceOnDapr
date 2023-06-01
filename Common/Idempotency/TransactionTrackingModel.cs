using System;
using Common.Entities;

namespace Common.Idempotency
{
    /**
     * Base class for microservices to handle idempotent operations
     * due to weak delivery guarantees
     */
    public record TransactionTrackingModel(string instanceId, DateTime createdAt);
    
}