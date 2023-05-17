using System;
namespace Common.Entities
{
    public struct ProductStatus
    {
        public long Id { get; }
        public ItemStatus Status { get; }
        public decimal UnitPrice { get; }
        public decimal OldUnitPrice { get; }

        public ProductStatus(long id, ItemStatus status, decimal price, decimal oldPrice)
        {
            this.Id = id;
            this.Status = status;
            this.UnitPrice = price;
            this.OldUnitPrice = oldPrice;
        }
    }
}

