using System;
using Common.Events;

namespace ShipmentMS.Service
{
	public interface IShipmentService
	{
        public Task ProcessShipment(PaymentConfirmation paymentResult);

        public void UpdateShipment(string instanceId = "");
    }
}

