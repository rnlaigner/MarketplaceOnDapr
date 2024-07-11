﻿using Microsoft.EntityFrameworkCore.Storage;
using ProductMS.Models;

namespace ProductMS.Repositories;

public interface IProductRepository
{
    void Update(ProductModel product);

    void Insert(ProductModel product);

    ProductModel? GetProduct(int sellerId, int productId);

    ProductModel GetProductForUpdate(int sellerId, int productId);

    IEnumerable<ProductModel> GetBySeller(int sellerId);

    // APIs for ProductService
    IDbContextTransaction BeginTransaction();
    void Reset();
    void Cleanup();

}

