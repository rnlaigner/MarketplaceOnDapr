using System;
using Common.Entities;
using Common.Events;
using ProductMS.Models;

namespace ProductMS.Services
{
	public interface IProductService
	{
        public Task<bool> Upsert(Product productToUpdate);
        public Task<bool> Delete(ProductModel productToDelete);

    }
}

