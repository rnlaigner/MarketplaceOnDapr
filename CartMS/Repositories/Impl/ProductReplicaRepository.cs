using CartMS.Infra;
using CartMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CartMS.Repositories.Impl;

public class ProductReplicaRepository : IProductReplicaRepository
{
    private readonly CartDbContext dbContext;

    private readonly ILogger<ProductReplicaRepository> logger;

    public ProductReplicaRepository(CartDbContext dbContext, ILogger<ProductReplicaRepository> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public ProductReplicaModel GetProduct(int sellerId, int productId)
    {
        return this.dbContext.Products.Where(p=> p.seller_id == sellerId && p.product_id == productId).First();
    }

    public bool Exists(int sellerId, int productId)
    {
        return this.dbContext.Products.Find(sellerId, productId) != null;
    }
        
    private const string SELECT_PRODUCT_FOR_UPDATE = "SELECT * FROM cart.replica_products s WHERE s.seller_id = {0} AND s.product_id = {1} FOR UPDATE";

    public ProductReplicaModel GetProductForUpdate(int sellerId, int productId)
    {
        var sql = string.Format(SELECT_PRODUCT_FOR_UPDATE, sellerId, productId);
        return this.dbContext.Products.FromSqlRaw(sql).First();
    }

    public IList<ProductReplicaModel> GetProducts(IList<(int, int)> ids)
    {
        List<ProductReplicaModel> products = new List<ProductReplicaModel>();
        foreach(var entry in ids)
        {
            var res = this.dbContext.Products.Find(entry.Item1, entry.Item2);
            if (res is not null)
                products.Add(res);
        }
        return products;
    }

    public ProductReplicaModel Insert(ProductReplicaModel product)
    {
        product.updated_at = DateTime.UtcNow;
        product.created_at = product.updated_at;
        var track = this.dbContext.Products.Add(product);
        this.dbContext.SaveChanges();
        return track.Entity;
    }

    public ProductReplicaModel Update(ProductReplicaModel product)
    {
        product.updated_at = DateTime.UtcNow;
        var track = this.dbContext.Products.Update(product);
        this.dbContext.SaveChanges();
        return track.Entity;
    }

    public void Reset()
    {
        this.dbContext.Database.ExecuteSqlRaw("UPDATE cart.replica_products SET active=true, version='0'");
        this.dbContext.SaveChanges();
    }

    public void Cleanup()
    {
        this.dbContext.Products.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

}

