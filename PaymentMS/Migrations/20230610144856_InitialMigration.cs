using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_payments",
                columns: table => new
                {
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_sequential = table.Column<int>(type: "integer", nullable: false),
                    payment_type = table.Column<string>(type: "text", nullable: false),
                    payment_installments = table.Column<int>(type: "integer", nullable: false),
                    payment_value = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_payments", x => new { x.order_id, x.payment_sequential });
                    table.CheckConstraint("CK_OrderPayment_PaymentValue", "payment_value >= 0");
                });

            migrationBuilder.CreateTable(
                name: "order_payment_cards",
                columns: table => new
                {
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_sequential = table.Column<int>(type: "integer", nullable: false),
                    card_number = table.Column<string>(type: "text", nullable: false),
                    card_holder_name = table.Column<string>(type: "text", nullable: false),
                    card_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    card_brand = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_payment_cards", x => new { x.order_id, x.payment_sequential });
                    table.ForeignKey(
                        name: "FK_order_payment_cards_order_payments_order_id_payment_sequent~",
                        columns: x => new { x.order_id, x.payment_sequential },
                        principalTable: "order_payments",
                        principalColumns: new[] { "order_id", "payment_sequential" });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_payment_cards");

            migrationBuilder.DropTable(
                name: "order_payments");
        }
    }
}
