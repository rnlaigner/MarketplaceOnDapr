﻿using System;
using Common.Events;

namespace ShipmentMS.Service
{
	public interface IShipmentService
	{
        public Task ProcessShipment(PaymentConfirmed paymentResult);

        public Task UpdateShipment(int instanceId);
    }
}

