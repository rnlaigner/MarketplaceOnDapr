﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockMS.Infra;

#nullable disable

namespace StockMS.Migrations
{
    [DbContext(typeof(StockDbContext))]
    partial class StockDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StockMS.Models.StockItemModel", b =>
                {
                    b.Property<long>("product_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("product_id"));

                    b.Property<bool>("active")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("data")
                        .HasColumnType("text");

                    b.Property<int>("order_count")
                        .HasColumnType("integer");

                    b.Property<int>("qty_available")
                        .HasColumnType("integer");

                    b.Property<int>("qty_reserved")
                        .HasColumnType("integer");

                    b.Property<long>("seller_id")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("updated_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("ytd")
                        .HasColumnType("integer");

                    b.HasKey("product_id");

                    b.ToTable("stock_items", t =>
                        {
                            t.HasCheckConstraint("CK_StockItem_QtyAvailable", "qty_available >= 0");

                            t.HasCheckConstraint("CK_StockItem_QtyReserved", "qty_reserved >= 0");

                            t.HasCheckConstraint("CK_StockItem_QtyReservedLessThanQtyAvailable", "qty_reserved <= qty_available");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
