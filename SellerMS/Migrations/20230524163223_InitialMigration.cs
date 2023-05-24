using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SellerMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_entry_details",
                columns: table => new
                {
                    order_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    street = table.Column<string>(type: "text", nullable: false),
                    complement = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    zip_code = table.Column<string>(type: "text", nullable: false),
                    card_brand = table.Column<string>(type: "text", nullable: false),
                    installments = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_entry_details", x => x.order_id);
                });

            migrationBuilder.CreateTable(
                name: "product_entry_view",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    qty_available = table.Column<int>(type: "integer", nullable: false),
                    qty_reserved = table.Column<int>(type: "integer", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    total_revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    total_discount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_entry_view", x => x.product_id);
                });

            migrationBuilder.CreateTable(
                name: "shipment_entry_view",
                columns: table => new
                {
                    seller_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    package_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_entry_view", x => x.seller_id);
                });

            migrationBuilder.CreateTable(
                name: "shipment_view",
                columns: table => new
                {
                    seller_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    total_number = table.Column<decimal>(type: "numeric", nullable: false),
                    avg_mean_time_to_complete = table.Column<long>(type: "bigint", nullable: false),
                    avg_shipment_value_per_order = table.Column<decimal>(type: "numeric", nullable: false),
                    avg_shipment_value_per_item = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_view", x => x.seller_id);
                });

            migrationBuilder.CreateTable(
                name: "order_entry_view",
                columns: table => new
                {
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    count_items = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_freight = table.Column<decimal>(type: "numeric", nullable: false),
                    total_incentive = table.Column<decimal>(type: "numeric", nullable: false),
                    total_invoice = table.Column<decimal>(type: "numeric", nullable: false),
                    total_items = table.Column<decimal>(type: "numeric", nullable: false),
                    count_vouchers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_entry_view", x => new { x.order_id, x.seller_id });
                    table.ForeignKey(
                        name: "FK_order_entry_view_order_entry_details_order_id",
                        column: x => x.order_id,
                        principalTable: "order_entry_details",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_entry_view_seller_id",
                table: "order_entry_view",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entry_view_seller_id",
                table: "product_entry_view",
                column: "seller_id");

            migrationBuilder.Sql("CREATE FUNCTION \"LC_TRIGGER_AFTER_INSERT_ORDERENTRYVIEWMODEL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_ORDERENTRYVIEWMODEL$\r\nBEGIN\r\n  REFRESH MATERIALIZED VIEW CONCURRENTLY OrderViewModel\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_ORDERENTRYVIEWMODEL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_ORDERENTRYVIEWMODEL AFTER INSERT\r\nON \"order_entry_view\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"LC_TRIGGER_AFTER_INSERT_ORDERENTRYVIEWMODEL\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"LC_TRIGGER_AFTER_INSERT_ORDERENTRYVIEWMODEL\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "order_entry_view");

            migrationBuilder.DropTable(
                name: "product_entry_view");

            migrationBuilder.DropTable(
                name: "shipment_entry_view");

            migrationBuilder.DropTable(
                name: "shipment_view");

            migrationBuilder.DropTable(
                name: "order_entry_details");
        }
    }
}
