using System;
using Common.Entities;
using Common.Events;
using Common.Integration;
using Common.Requests;
using ProductMS.Models;

namespace ProductMS.Services
{
	public interface IProductService
	{
        Task ProcessNewProduct(Product productToUpdate);
        Task ProcessDelete(DeleteProduct productToDelete);
        Task ProcessUpdate(UpdatePrice update);
        void Cleanup();
    }
}