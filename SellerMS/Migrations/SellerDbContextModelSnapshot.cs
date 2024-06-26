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
                .HasDefaultSchema("seller")
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SellerMS.Models.OrderEntry", b =>
                {
                    b.Property<int>("order_id")
                        .HasColumnType("integer");

                    b.Property<int>("product_id")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("delivery_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("delivery_status")
                        .HasColumnType("text");

                    b.Property<float>("freight_value")
                        .HasColumnType("real");

                    b.Property<string>("order_status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("package_id")
                        .HasColumnType("integer");

                    b.Property<string>("product_category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("product_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("quantity")
                        .HasColumnType("integer");

                    b.Property<int>("seller_id")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("shipment_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<float>("total_amount")
                        .HasColumnType("real");

                    b.Property<float>("total_incentive")
                        .HasColumnType("real");

                    b.Property<float>("total_invoice")
                        .HasColumnType("real");

                    b.Property<float>("total_items")
                        .HasColumnType("real");

                    b.Property<float>("unit_price")
                        .HasColumnType("real");

                    b.HasKey("order_id", "product_id");

                    b.HasIndex("seller_id");

                    b.HasIndex(new[] { "order_status" }, "order_entry_open_idx")
                        .HasFilter("order_status = 'INVOICED' OR order_status = 'PAYMENT_PROCESSED' OR order_status = 'READY_FOR_SHIPMENT' OR order_status = 'IN_TRANSIT'");

                    b.ToTable("order_entries", "seller");
                });

            modelBuilder.Entity("SellerMS.Models.OrderEntryDetails", b =>
                {
                    b.Property<int>("order_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("order_id"));

                    b.Property<string>("card_brand")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("city")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("complement")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("customer_id")
                        .HasColumnType("integer");

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

                    b.ToTable("order_entry_details", "seller", t =>
                        {
                            t.HasTrigger("LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS");

                            t.HasTrigger("LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS");
                        });

                    b
                        .HasAnnotation("LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS", "CREATE FUNCTION \"seller\".\"LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS$\r\nBEGIN\r\n  REFRESH MATERIALIZED VIEW CONCURRENTLY OrderSellerView;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS AFTER INSERT\r\nON \"seller\".\"order_entry_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"seller\".\"LC_TRIGGER_AFTER_INSERT_ORDERENTRYDETAILS\"();")
                        .HasAnnotation("LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS", "CREATE FUNCTION \"seller\".\"LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS$\r\nBEGIN\r\n  REFRESH MATERIALIZED VIEW CONCURRENTLY OrderSellerView;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS AFTER UPDATE\r\nON \"seller\".\"order_entry_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"seller\".\"LC_TRIGGER_AFTER_UPDATE_ORDERENTRYDETAILS\"();");
                });

            modelBuilder.Entity("SellerMS.Models.OrderSellerView", b =>
                {
                    b.Property<int>("count_items")
                        .HasColumnType("integer");

                    b.Property<int>("seller_id")
                        .HasColumnType("integer");

                    b.Property<float>("total_amount")
                        .HasColumnType("real");

                    b.Property<float>("total_freight")
                        .HasColumnType("real");

                    b.Property<float>("total_incentive")
                        .HasColumnType("real");

                    b.Property<float>("total_invoice")
                        .HasColumnType("real");

                    b.Property<float>("total_items")
                        .HasColumnType("real");

                    b.ToTable((string)null);

                    b.ToView("OrderSellerView", "seller");
                });

            modelBuilder.Entity("SellerMS.Models.SellerModel", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("city")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("cnpj")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("company_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("complement")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("cpf")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("mobile_phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("state")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("zip_code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("sellers", "seller");
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
