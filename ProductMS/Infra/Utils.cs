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
                product_id = product.product_id,
                seller_id = product.seller_id,
                name = product.name,
                sku = product.sku,
                category = product.category,
                description = product.description,
                price = product.price,
                created_at = product.created_at is null ? DateTime.Now : product.created_at.Value,
                status = product.status
            };

        }

        public static Product AsProduct(ProductModel product)
        {
            return new()
            {
                product_id = product.product_id,
                seller_id = product.seller_id,
                name = product.name,
                sku = product.sku,
                category = product.category,
                description = product.description,
                price = product.price,
                created_at = product.created_at,
                updated_at = product.updated_at,
                status = product.status,
                active = product.active
            };

        }

    }
}

