using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentMS.Migrations
{
    /// <inheritdoc />
    public partial class PaymentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payment");

            migrationBuilder.CreateTable(
                name: "order_payments",
                schema: "payment",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    sequential = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    installments = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<float>(type: "real", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_payments", x => new { x.customer_id, x.order_id, x.sequential });
                    table.CheckConstraint("CK_OrderPayment_Value", "value >= 0");
                });

            migrationBuilder.CreateTable(
                name: "order_payment_cards",
                schema: "payment",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    sequential = table.Column<int>(type: "integer", nullable: false),
                    card_number = table.Column<string>(type: "text", nullable: false),
                    card_holder_name = table.Column<string>(type: "text", nullable: false),
                    card_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    card_brand = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_payment_cards", x => new { x.customer_id, x.order_id, x.sequential });
                    table.ForeignKey(
                        name: "FK_order_payment_cards_order_payments_customer_id_order_id_seq~",
                        columns: x => new { x.customer_id, x.order_id, x.sequential },
                        principalSchema: "payment",
                        principalTable: "order_payments",
                        principalColumns: new[] { "customer_id", "order_id", "sequential" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_payment_cards",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "order_payments",
                schema: "payment");
        }
    }
}
