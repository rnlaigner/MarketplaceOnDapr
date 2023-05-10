using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrderMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "OrderHistoryNumbers");

            migrationBuilder.CreateSequence(
                name: "OrderNumbers");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"OrderNumbers\"')")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    customer_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_carrier_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_customer_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estimated_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    count_items = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    total_freight = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    total_incentive = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    total_invoice = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    total_items = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    instanceId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"OrderHistoryNumbers\"')")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_history_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    order_item_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    shipping_limit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    freight_value = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    total_items = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => new { x.order_id, x.order_item_id });
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_history_order_id",
                table: "order_history",
                column: "order_id");

            // partitioning orders table
            migrationBuilder.Sql(
                @"
                    ALTER TABLE orders
                    ATTACH PARTITION orders_partition
                    PARTITION BY RANGE (id)
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_history");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropSequence(
                name: "OrderHistoryNumbers");

            migrationBuilder.DropSequence(
                name: "OrderNumbers");
        }
    }
}
