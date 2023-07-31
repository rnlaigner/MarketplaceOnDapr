using System;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SellerMS.Controllers;
using SellerMS.Infra;
using SellerMS.Models;
using SellerMS.Services;

namespace SellerMS.Repositories
{
	public class SellerRepository : ISellerRepository
    {

        private readonly SellerDbContext dbContext;

        private readonly ILogger<SellerRepository> logger;

        public SellerRepository(SellerDbContext sellerDbContext, ILogger<SellerRepository> logger)
		{
            this.dbContext = sellerDbContext;
            this.logger = logger;
        }

        public SellerModel Insert(SellerModel seller)
        {
            var entity = dbContext.Sellers.Add(seller).Entity;
            dbContext.SaveChanges();
            return entity;
        }

        public SellerModel? Get(int sellerId)
        {
            return dbContext.Sellers.Find(sellerId);
            
        }
    }

}