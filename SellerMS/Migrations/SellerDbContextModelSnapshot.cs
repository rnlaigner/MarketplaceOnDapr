﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SellerMS.Infra;

#nullable disable

namespace SellerMS.Migrations
{
    [DbContext(typeof(SellerDbContext))]
    partial class SellerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SellerMS.Models.OrderEntry", b =>
                {
                    b.Property<long>("order_id")
                        .HasColumnType("bigint");

                    b.Property<long>("product_id")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("delivery_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("delivery_status")
                        .HasColumnType("text");

                    b.Property<decimal>("freight_value")
                        .HasColumnType("numeric");

                    b.Property<string>("order_status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long?>("package_id")
                        .HasColumnType("bigint");

                    b.Property<string>("product_category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("product_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("quantity")
                        .HasColumnType("integer");

                    b.Property<long>("seller_id")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("shipment_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("total_amount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_incentive")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_invoice")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_items")
                        .HasColumnType("numeric");

                    b.Property<decimal>("unit_price")
                        .HasColumnType("numeric");

                    b.HasKey("order_id", "product_id");

                    b.HasIndex("seller_id");

                    b.HasIndex(new[] { "order_status" }, "order_entry_open_idx")
                        .HasFilter("order_status = 'INVOICED' OR order_status = 'PAYMENT_PROCESSED' OR order_status = 'READY_FOR_SHIPMENT' OR order_status = 'IN_TRANSIT'");

                    b.ToTable("order_entries");
                });

            modelBuilder.Entity("SellerMS.Models.OrderEntryDetails", b =>
                {
                    b.Property<long>("order_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("order_id"));

                    b.Property<string>("card_brand")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("city")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("complement")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("customer_id")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("first_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("installments")
                        .HasColumnType("integer");

                    b.Property<string>("last_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("order_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("state")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("street")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("zip_code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("order_id");

                    b.ToTable("order_entry_details", t =>
                        {
                            t.HasTrigger("LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS");

                            t.HasTrigger("LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS");
                        });

                    b
                        .HasAnnotation("LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS", "CREATE FUNCTION \"LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS$\r\nBEGIN\r\n  REFRESH MATERIALIZED VIEW CONCURRENTLY OrderSellerView;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS AFTER INSERT\r\nON \"order_entry_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS\"();")
                        .HasAnnotation("LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS", "CREATE FUNCTION \"LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS$\r\nBEGIN\r\n  REFRESH MATERIALIZED VIEW CONCURRENTLY OrderSellerView;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS AFTER UPDATE\r\nON \"order_entry_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS\"();");
                });

            modelBuilder.Entity("SellerMS.Models.OrderHistoricalView", b =>
                {
                    b.Property<decimal>("avg_order_revenue")
                        .HasColumnType("numeric");

                    b.Property<decimal>("avg_order_value")
                        .HasColumnType("numeric");

                    b.Property<int>("count_orders")
                        .HasColumnType("integer");

                    b.Property<decimal>("revenue")
                        .HasColumnType("numeric");

                    b.Property<long>("seller_id")
                        .HasColumnType("bigint");

                    b.Property<decimal>("total_overall")
                        .HasColumnType("numeric");

                    b.ToTable((string)null);

                    b.ToView("OrderHistoricalView", (string)null);
                });

            modelBuilder.Entity("SellerMS.Models.OrderSellerView", b =>
                {
                    b.Property<int>("count_items")
                        .HasColumnType("integer");

                    b.Property<long>("order_id")
                        .HasColumnType("bigint");

                    b.Property<long>("seller_id")
                        .HasColumnType("bigint");

                    b.Property<decimal>("total_amount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_freight")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_incentive")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_invoice")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_items")
                        .HasColumnType("numeric");

                    b.ToTable((string)null);

                    b.ToView("OrderSellerView", (string)null);
                });

            modelBuilder.Entity("SellerMS.Models.ShipmentHistoricalView", b =>
                {
                    b.Property<decimal>("avg_freight_value")
                        .HasColumnType("numeric");

                    b.Property<long>("avg_time_to_complete")
                        .HasColumnType("bigint");

                    b.Property<decimal>("count_shipments")
                        .HasColumnType("numeric");

                    b.Property<long>("seller_id")
                        .HasColumnType("bigint");

                    b.Property<decimal>("total_freight_amount")
                        .HasColumnType("numeric");

                    b.ToTable((string)null);

                    b.ToView("ShipmentHistoricalView", (string)null);
                });

            modelBuilder.Entity("SellerMS.Models.OrderEntry", b =>
                {
                    b.HasOne("SellerMS.Models.OrderEntryDetails", "details")
                        .WithMany()
                        .HasForeignKey("order_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("details");
                });
#pragma warning restore 612, 618
        }
    }
}
