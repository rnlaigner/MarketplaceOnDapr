using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;

namespace OrderMS.Common.Models
{
    // https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=data-annotations
    [Table("order_history", Schema = "order")]
    [PrimaryKey(nameof(id))]
    public class OrderHistoryModel
    {
        public int id { get; set; }

        public int order_id { get; set; }

        public DateTime created_at { get; set; }

        public OrderStatus status { get; set; }

        public OrderHistoryModel() { }

    }
}

