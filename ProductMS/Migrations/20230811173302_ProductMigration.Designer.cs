﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ProductMS.Infra;

#nullable disable

namespace ProductMS.Migrations
{
    [DbContext(typeof(ProductDbContext))]
    [Migration("20230811173302_ProductMigration")]
    partial class ProductMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("product")
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ProductMS.Models.ProductModel", b =>
                {
                    b.Property<int>("seller_id")
                        .IsRequired()
                        .HasColumnType("integer");

                    b.Property<int>("product_id")
                        .IsRequired()
                        .HasColumnType("integer");

                    b.Property<bool>("active")
                        .IsRequired()
                        .HasColumnType("boolean");

                    b.Property<string>("category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("created_at")
                        .IsRequired()
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float>("freight_value")
                        .IsRequired()
                        .HasColumnType("real");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float>("price")
                        .IsRequired()
                        .HasColumnType("real");

                    b.Property<string>("sku")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("updated_at")
                        .IsRequired()
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("version")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("seller_id", "product_id");

                    b.ToTable("products", "product", t =>
                        {
                            t.HasCheckConstraint("CK_Product_Price", "price >= 0");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
