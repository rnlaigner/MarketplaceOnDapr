using CartMS.Infra;
using CartMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CartMS.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext dbContext;

    private readonly ILogger<CartRepository> logger;

    public CartRepository(CartDbContext cartDbContext, ILogger<CartRepository> logger)
    {
        this.dbContext = cartDbContext;
        this.logger = logger;
    }

    public CartModel? GetCart(int customerId)
    {
        return this.dbContext.Carts.Find(customerId);
    }

    public CartModel? Delete(int customerId)
    {
        CartModel? cart = this.GetCart(customerId);
        if (cart is not null)
        {
            dbContext.Remove(cart);
            dbContext.SaveChanges();
            return cart;
        }

        var items = GetItems(customerId);
        this.dbContext.RemoveRange(items);
        this.dbContext.SaveChanges();

        return null;
    }

    public void Insert(CartModel cart)
    {
        this.dbContext.Add(cart);
        this.dbContext.SaveChanges();
    }

    public IList<CartItemModel> GetItems(int customerId)
    {
        return this.dbContext.CartItems.Where(c => c.customer_id == customerId).ToList();
    }

    public CartItemModel AddItem(CartItemModel item)
    {
        var res = this.dbContext.CartItems.Add(item);
        this.dbContext.SaveChanges();
        return res.Entity;  
    }

    public CartItemModel UpdateItem(CartItemModel item)
    {
        var entity = this.dbContext.CartItems.Update(item);
        this.dbContext.SaveChanges();
        return entity.Entity;
    }

    public void Update(CartModel cart)
    {
        cart.updated_at = DateTime.UtcNow;
        var f = this.dbContext.Update(cart);
        this.dbContext.SaveChanges();
    }

    private static readonly IList<CartItemModel> EMPTY_LIST = new List<CartItemModel>();

    public IList<CartItemModel> GetItemsByProduct(int sellerId, int productId, string version)
    {
        var items = this.dbContext.CartItems.Where(p=> p.seller_id == sellerId && p.product_id == productId && p.version.Contains(version));
        if(items.Any()) return items.ToList();
        return EMPTY_LIST;
    }

    public void Cleanup()
    {
        this.dbContext.Carts.ExecuteDelete();
        this.dbContext.CartItems.ExecuteDelete();
        this.dbContext.Products.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

    public void Reset()
    {
        this.dbContext.Carts.ExecuteDelete();
        this.dbContext.CartItems.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

    public IDbContextTransaction BeginTransaction()
    {
        return this.dbContext.Database.BeginTransaction();
    }

    public void FlushUpdates()
    {
        this.dbContext.SaveChanges();
    }
}

