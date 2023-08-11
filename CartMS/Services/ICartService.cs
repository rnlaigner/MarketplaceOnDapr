using CartMS.Models;
using Common.Driver;
using Common.Events;
using Common.Requests;

namespace CartMS.Services;

public interface ICartService
{
    // can also be used for test
    void Seal(CartModel cart, bool cleanItems = true);

    Task<bool> NotifyCheckout(CustomerCheckout customerCheckout);

    void Cleanup();

    Task ProcessPriceUpdate(PriceUpdated updatePrice);

    void Reset();

    Task ProcessPoisonPriceUpdate(PriceUpdated update);
    Task ProcessPoisonCheckout(CustomerCheckout customerCheckout, MarkStatus status);
}

