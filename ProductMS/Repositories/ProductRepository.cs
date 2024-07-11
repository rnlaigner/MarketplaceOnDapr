using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductMS.Infra;
using ProductMS.Models;

namespace ProductMS.Repositories;

public class ProductRepository : IProductRepository
{

    private readonly ProductDbContext dbContext;

    public ProductRepository(ProductDbContext dbContext)
	{
        this.dbContext = dbContext;
    }

    public IEnumerable<ProductModel> GetBySeller(int sellerId)
    {
        return this.dbContext.Products.Where(p => p.seller_id == sellerId);
    }

    public ProductModel? GetProduct(int sellerId, int productId)
    {
        return this.dbContext.Products.Find(sellerId, productId);
    }

    private const string SELECT_PRODUCT_FOR_UPDATE = "SELECT * FROM product.products s WHERE s.seller_id = {0} AND s.product_id = {1} FOR UPDATE";

    public ProductModel GetProductForUpdate(int sellerId, int productId)
    {
        var sql = string.Format(SELECT_PRODUCT_FOR_UPDATE, sellerId, productId);
        return this.dbContext.Products.FromSqlRaw(sql).First();
    }

    public void Insert(ProductModel product)
    {
        product.created_at = DateTime.UtcNow;
        product.updated_at = product.created_at;
        product.active = true;
        this.dbContext.Products.Add(product);
        this.dbContext.SaveChanges();
    }

    public void Update(ProductModel product)
    {
        product.updated_at = DateTime.UtcNow;
        product.active = true;
        this.dbContext.Products.Update(product);
        this.dbContext.SaveChanges();
    }

    public IDbContextTransaction BeginTransaction()
    {
        return this.dbContext.Database.BeginTransaction();
    }

    public void Reset()
    {
        this.dbContext.Database.ExecuteSqlRaw("UPDATE product.products SET active=true, version='0'");
        this.dbContext.SaveChanges();
    }

    public void Cleanup()
    {
        this.dbContext.Products.ExecuteDelete();
        this.dbContext.SaveChanges();
    }
}

