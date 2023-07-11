﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrderMS.Infra;

#nullable disable

namespace OrderMS.Migrations
{
    [DbContext(typeof(OrderDbContext))]
    [Migration("20230609114239_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence("OrderHistoryNumbers");

            modelBuilder.HasSequence("OrderNumbers");

            modelBuilder.Entity("OrderMS.Common.Models.CustomerOrderModel", b =>
                {
                    b.Property<long>("customer_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("customer_id"));

                    b.Property<long>("next_order_id")
                        .HasColumnType("bigint");

                    b.HasKey("customer_id");

                    b.ToTable("customer_orders");
                });

            modelBuilder.Entity("OrderMS.Common.Models.OrderHistoryModel", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValueSql("nextval('\"OrderHistoryNumbers\"')");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("id"));

                    b.Property<DateTime>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("order_id")
                        .HasColumnType("bigint");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.HasIndex("order_id");

                    b.ToTable("order_history");
                });

            modelBuilder.Entity("OrderMS.Common.Models.OrderItemModel", b =>
                {
                    b.Property<long>("order_id")
                        .HasColumnType("bigint");

                    b.Property<long>("order_item_id")
                        .HasColumnType("bigint");

                    b.Property<decimal>("freight_value")
                        .HasColumnType("numeric");

                    b.Property<long>("product_id")
                        .HasColumnType("bigint");

                    b.Property<string>("product_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("quantity")
                        .HasColumnType("integer");

                    b.Property<long>("seller_id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("shipping_limit_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("total_amount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("total_items")
                        .HasColumnType("numeric");

                    b.Property<decimal>("unit_price")
                        .HasColumnType("numeric");

                    b.HasKey("order_id", "order_item_id");

                    b.ToTable("order_items");
                });

            modelBuilder.Entity("OrderMS.Common.Models.OrderModel", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValueSql("nextval('\"OrderNumbers\"')");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("id"));

                    b.Property<int>("count_items")
                        .HasColumnType("integer");

                    b.Property<DateTime>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("customer_id")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("delivered_carrier_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("delivered_customer_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("estimated_delivery_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("invoice_number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("payment_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("purchase_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("text");

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

                    b.Property<DateTime>("updated_at")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("id");

                    b.HasIndex("customer_id");

                    b.ToTable("orders");
                });

            modelBuilder.Entity("OrderMS.Common.Models.OrderHistoryModel", b =>
                {
                    b.HasOne("OrderMS.Common.Models.OrderModel", null)
                        .WithMany("history")
                        .HasForeignKey("order_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OrderMS.Common.Models.OrderItemModel", b =>
                {
                    b.HasOne("OrderMS.Common.Models.OrderModel", null)
                        .WithMany("items")
                        .HasForeignKey("order_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OrderMS.Common.Models.OrderModel", b =>
                {
                    b.Navigation("history");

                    b.Navigation("items");
                });
#pragma warning restore 612, 618
        }
    }
}