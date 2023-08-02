using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SellerMS.Migrations
{
    /// <inheritdoc />
    public partial class SellerMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "seller");

            migrationBuilder.CreateTable(
                name: "order_entry_details",
                schema: "seller",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
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
                name: "sellers",
                schema: "seller",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    mobile_phone = table.Column<string>(type: "text", nullable: false),
                    cpf = table.Column<string>(type: "text", nullable: false),
                    cnpj = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    complement = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    zip_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sellers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_entries",
                schema: "seller",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    seller_id = table.Column<int>(type: "integer", nullable: false),
                    package_id = table.Column<int>(type: "integer", nullable: true),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    product_category = table.Column<string>(type: "text", nullable: false),
                    unit_price = table.Column<float>(type: "real", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    total_items = table.Column<float>(type: "real", nullable: false),
                    total_amount = table.Column<float>(type: "real", nullable: false),
                    total_incentive = table.Column<float>(type: "real", nullable: false),
                    total_invoice = table.Column<float>(type: "real", nullable: false),
                    freight_value = table.Column<float>(type: "real", nullable: false),
                    shipment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_status = table.Column<string>(type: "text", nullable: false),
                    delivery_status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_entries", x => new { x.order_id, x.product_id });
                    table.ForeignKey(
                        name: "FK_order_entries_order_entry_details_order_id",
                        column: x => x.order_id,
                        principalSchema: "seller",
                        principalTable: "order_entry_details",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_entries_seller_id",
                schema: "seller",
                table: "order_entries",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "order_entry_open_idx",
                schema: "seller",
                table: "order_entries",
                column: "order_status",
                filter: "order_status = 'INVOICED' OR order_status = 'PAYMENT_PROCESSED' OR order_status = 'READY_FOR_SHIPMENT' OR order_status = 'IN_TRANSIT'");

            migrationBuilder.Sql("CREATE FUNCTION \"seller\".\"LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS$\r\nBEGIN\r\n  REFRESH MATERIALIZED VIEW CONCURRENTLY OrderSellerView;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS AFTER INSERT\r\nON \"seller\".\"order_entry_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"seller\".\"LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"seller\".\"LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS$\r\nBEGIN\r\n  REFRESH MATERIALIZED VIEW CONCURRENTLY OrderSellerView;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS AFTER UPDATE\r\nON \"seller\".\"order_entry_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"seller\".\"LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"seller\".\"LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"seller\".\"LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "order_entries",
                schema: "seller");

            migrationBuilder.DropTable(
                name: "sellers",
                schema: "seller");

            migrationBuilder.DropTable(
                name: "order_entry_details",
                schema: "seller");
        }
    }
}
