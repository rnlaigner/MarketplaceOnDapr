using System;
using SellerMS.Models;

namespace SellerMS.DTO
{
	public record SellerDashboard
	(
		OrderSellerView sellerView,
		IList<OrderEntry> orderEntries //,
		// IList<ProductEntry> productEntries 
	);
}

