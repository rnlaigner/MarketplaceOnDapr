using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMS.Migrations
{
    /// <inheritdoc />
    public partial class StockMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stock");

            migrationBuilder.CreateTable(
                name: "stock_items",
                schema: "stock",
                columns: table => new
                {
                    seller_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    qty_available = table.Column<int>(type: "integer", nullable: false),
                    qty_reserved = table.Column<int>(type: "integer", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    ytd = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_items", x => new { x.seller_id, x.product_id });
                    table.CheckConstraint("CK_StockItem_QtyAvailable", "qty_available >= 0");
                    table.CheckConstraint("CK_StockItem_QtyReserved", "qty_reserved >= 0");
                    table.CheckConstraint("CK_StockItem_QtyReservedLessThanQtyAvailable", "qty_reserved <= qty_available");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_items",
                schema: "stock");
        }
    }
}
