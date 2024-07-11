﻿using Common.Entities;
using Common.Events;

namespace StockMS.Services
{
	public interface IStockService
	{
        Task ReserveStockAsync(ReserveStock checkout);

        void ConfirmReservation(PaymentConfirmed payment);

        void CancelReservation(PaymentFailed paymentFailure);

        Task ProcessProductUpdate(ProductUpdated productUpdate);

        Task CreateStockItem(StockItem stockItem);

        Task IncreaseStock(IncreaseStock increaseStock);

        void Cleanup();
        void Reset();

        Task ProcessPoisonReserveStock(ReserveStock reserveStock);
        Task ProcessPoisonProductUpdate(ProductUpdated productUpdate);

    }
}

