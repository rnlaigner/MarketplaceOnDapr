using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;

namespace OrderMS.Common.Models
{
    // https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=data-annotations
    // [Index(nameof(instanceId), IsUnique = false)]

    [Table("order_history")]
    [PrimaryKey(nameof(id))]
    public class OrderHistoryModel
    {
        public long id { get; set; }

        public long order_id { get; set; }

        // public long item_id { get; set; }

        public DateTime created_at { get; set; }

        public OrderStatus? orderStatus { get; set; }

        // for idempotency of delivery updates
        // public string instanceId { get; set; }

        // public PackageStatus? packageStatus { get; set; }

        // public string? data { get; set; }

        public OrderHistoryModel() { }

    }
}

