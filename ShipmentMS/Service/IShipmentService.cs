using System;
using Common.Events;

namespace ShipmentMS.Service
{
	public interface IShipmentService
	{
        public void ProcessShipment(PaymentResult paymentRequest);

        public void UpdateShipment();
    }
}

