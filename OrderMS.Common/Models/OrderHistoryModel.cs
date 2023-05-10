using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;

namespace OrderMS.Common.Models
{
    [Table("order_history")]
    [PrimaryKey(nameof(id))]
    public class OrderHistoryModel
    {
        public long id { get; set; }

        public long order_id { get; set; }

        public DateTime created_at { get; set; }

        public OrderStatus status { get; set; }

        public OrderHistoryModel() { }

    }
}

