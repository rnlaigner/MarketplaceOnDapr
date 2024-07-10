using SellerMS.Infra;
using SellerMS.Models;

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
            var entity = this.dbContext.Sellers.Add(seller).Entity;
            this.dbContext.SaveChanges();
            return entity;
        }

        public SellerModel? Get(int sellerId)
        {
            return dbContext.Sellers.Find(sellerId);
            
        }
    }

}