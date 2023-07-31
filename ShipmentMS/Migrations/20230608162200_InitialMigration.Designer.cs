﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ShipmentMS.Infra;

#nullable disable

namespace ShipmentMS.Migrations
{
    [DbContext(typeof(ShipmentDbContext))]
    [Migration("20230608162200_InitialMigration")]
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

            modelBuilder.Entity("ShipmentMS.Models.PackageModel", b =>
                {
                    b.Property<int>("order_id")
                        .HasColumnType("integer");

                    b.Property<int>("package_id")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("delivery_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<float>("freight_value")
                        .HasPrecision(4, 2)
                        .HasColumnType("float");

                    b.Property<int>("product_id")
                        .HasColumnType("integer");

                    b.Property<string>("product_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("quantity")
                        .HasColumnType("integer");

                    b.Property<int>("seller_id")
                        .HasColumnType("integer");

                    b.Property<DateTime>("shipping_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("order_id", "package_id");

                    b.ToTable("packages");
                });

            modelBuilder.Entity("ShipmentMS.Models.ShipmentModel", b =>
                {
                    b.Property<int>("order_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("order_id"));

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

                    b.Property<string>("last_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("package_count")
                        .HasColumnType("integer");

                    b.Property<DateTime>("request_date")
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

                    b.Property<float>("total_freight_value")
                        .HasPrecision(4, 2)
                        .HasColumnType("float");

                    b.Property<string>("zip_code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("order_id");

                    b.ToTable("shipments");
                });

            modelBuilder.Entity("ShipmentMS.Models.PackageModel", b =>
                {
                    b.HasOne("ShipmentMS.Models.ShipmentModel", null)
                        .WithMany("packages")
                        .HasForeignKey("order_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ShipmentMS.Models.ShipmentModel", b =>
                {
                    b.Navigation("packages");
                });
#pragma warning restore 612, 618
        }
    }
}
