using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockMS.Models
{
    /**
	 * Track operations performed against the Stock state
	 * Besides, serves to guarantee idempotency in case events are duplicated
	 */
    [Table("stock_tracking")]
    [PrimaryKey(nameof(instanceId))]
    public record StockTracking
	(
		string instanceId, // workflow id
		OperationType operation, // reserve, cancel, confirm, add stock
		bool success = true
	);
}

