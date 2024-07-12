using SellerMS.Models;
using SellerMS.DTO;
using Microsoft.EntityFrameworkCore.Storage;

namespace SellerMS.Repositories;

public interface ISellerRepository
{
	SellerModel Insert(SellerModel seller);
	SellerModel? Get(int sellerId);

	// APIs for SellerService
    IDbContextTransaction BeginTransaction();
	void FlushUpdates();
    void Reset();
    void Cleanup();
	void RefreshSellerViewSafely();
	SellerDashboard QueryDashboard(int sellerId);
	void AddOrderEntry(OrderEntry orderEntry);
	void Update(OrderEntry orderEntry);
	void UpdateRange(IEnumerable<OrderEntry> orderEntries);
	IEnumerable<OrderEntry> GetOrderEntries(int customerId, int orderId);
	OrderEntry? Find(int customerId, int orderId, int sellerId, int productId);

}

