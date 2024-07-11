using System.Collections.Concurrent;
using CartMS.Models;

namespace CartMS.Repositories;

public class InMemoryProductReplicaRepository : IProductReplicaRepository
{
    private readonly ConcurrentDictionary<(int sellerId, int productId),ProductReplicaModel> productReplicas;

	public InMemoryProductReplicaRepository()
	{
        this.productReplicas = new();
	}

    public bool Exists(int sellerId, int productId)
    {
        return this.productReplicas.ContainsKey((sellerId,productId));
    }

    public ProductReplicaModel GetProduct(int sellerId, int productId)
    {
        return this.productReplicas[(sellerId,productId)];
    }

    public ProductReplicaModel GetProductForUpdate(int sellerId, int productId)
    {
        return this.GetProduct(sellerId, productId);
    }

    public IList<ProductReplicaModel> GetProducts(IList<(int, int)> ids)
    {
        List<ProductReplicaModel> res = new();
        foreach(var id in ids)
        {
            res.Add( this.GetProduct(id.Item1, id.Item2) );
        }
        return res;
    }

    public ProductReplicaModel Insert(ProductReplicaModel product)
    {
        this.productReplicas.TryAdd((product.seller_id, product.product_id), product);
        return product;
    }

    public ProductReplicaModel Update(ProductReplicaModel product)
    {
        this.productReplicas[(product.seller_id, product.product_id)] = product;
        return product;
    }

}

