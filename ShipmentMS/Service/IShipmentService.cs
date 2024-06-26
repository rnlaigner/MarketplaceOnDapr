using Common.Events;

namespace ShipmentMS.Service;

public interface IShipmentService
{
    public Task ProcessShipment(PaymentConfirmed paymentResult);

    public Task UpdateShipment(string instanceId);

    void Cleanup();

    Task ProcessPoisonShipment(PaymentConfirmed paymentRequest);
}

