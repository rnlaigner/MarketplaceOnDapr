using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{
    [Table("shipment_entry_view")]
    [PrimaryKey(nameof(seller_id))]
    public class ShipmentEntryViewModel
    {
		public long seller_id { get; set; }
		public long package_id { get; set; }



        public ShipmentEntryViewModel()
		{
		}
	}
}

