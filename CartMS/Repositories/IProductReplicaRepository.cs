﻿using CartMS.Models;

namespace CartMS.Repositories;

/**
* To address the product replication or state sharing
*/
public interface IProductReplicaRepository
{
	ProductReplicaModel Insert(ProductReplicaModel product);

	bool Exists(int sellerId, int productId);

	ProductReplicaModel Update(ProductReplicaModel product);

	IList<ProductReplicaModel> GetProducts(IList<(int, int)> ids);

    ProductReplicaModel GetProduct(int sellerId, int productId);

	ProductReplicaModel GetProductForUpdate(int sellerId, int productId);
}

