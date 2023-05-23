using System;
using Common.Entities;
using ProductMS.Models;

namespace ProductMS.Infra
{
	public sealed class Utils
	{
        public static ProductModel AsProductModel(Product product)
        {
            return new()
            {
                id = product.id,
                seller_id = product.seller_id,
                name = product.name,
                sku = product.sku,
                category = product.category_name,
                description = product.description,
                price = product.price,
                created_at = DateTime.Now,
            };

        }
    }
}

