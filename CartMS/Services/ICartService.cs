using CartMS.Models;
using Common.Driver;
using Common.Events;
using Common.Requests;

namespace CartMS.Services;

public interface ICartService
{
    // can also be used for test
    void Seal(CartModel cart, bool cleanItems = true);

    Task NotifyCheckout(CustomerCheckout customerCheckout);

    void Cleanup();

    void ProcessProductUpdated(ProductUpdated productUpdated);
    Task ProcessPriceUpdate(PriceUpdated updatePrice);

    void Reset();

    Task ProcessPoisonProductUpdated(ProductUpdated productUpdated);
    Task ProcessPoisonPriceUpdate(PriceUpdated update);
    Task ProcessPoisonCheckout(CustomerCheckout customerCheckout, MarkStatus status);
}

