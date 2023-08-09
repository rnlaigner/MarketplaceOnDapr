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

    Task ProcessProductUpdate(ProductUpdate updatePrice);

    void Reset();

    Task ProcessPoisonProductUpdate(ProductUpdate update);
    Task ProcessPoisonCheckout(CustomerCheckout customerCheckout, MarkStatus status);
}

