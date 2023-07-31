using SellerMS.Models;

namespace SellerMS.Repositories
{
	public interface ISellerRepository
	{
		SellerModel Insert(SellerModel seller);
		SellerModel? Get(int sellerId);
	}
}

