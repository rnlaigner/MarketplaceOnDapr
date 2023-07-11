﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PaymentMS.Infra;

#nullable disable

namespace PaymentMS.Migrations
{
    [DbContext(typeof(PaymentDbContext))]
    [Migration("20230612085819_InitialMigration")]
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

            modelBuilder.Entity("PaymentMS.Models.OrderPaymentCardModel", b =>
                {
                    b.Property<long>("order_id")
                        .HasColumnType("bigint");

                    b.Property<int>("payment_sequential")
                        .HasColumnType("integer");

                    b.Property<string>("card_brand")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("card_expiration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("card_holder_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("card_number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("order_id", "payment_sequential");

                    b.ToTable("order_payment_cards");
                });

            modelBuilder.Entity("PaymentMS.Models.OrderPaymentModel", b =>
                {
                    b.Property<long>("order_id")
                        .HasColumnType("bigint");

                    b.Property<int>("sequential")
                        .HasColumnType("integer");

                    b.Property<DateTime>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("installments")
                        .HasColumnType("integer");

                    b.Property<string>("status")
                        .HasColumnType("text");

                    b.Property<string>("type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("value")
                        .HasColumnType("numeric");

                    b.HasKey("order_id", "sequential");

                    b.ToTable("order_payments", t =>
                        {
                            t.HasCheckConstraint("CK_OrderPayment_Value", "value >= 0");
                        });
                });

            modelBuilder.Entity("PaymentMS.Models.OrderPaymentCardModel", b =>
                {
                    b.HasOne("PaymentMS.Models.OrderPaymentModel", "orderPayment")
                        .WithOne("orderPaymentCard")
                        .HasForeignKey("PaymentMS.Models.OrderPaymentCardModel", "order_id", "payment_sequential");

                    b.Navigation("orderPayment");
                });

            modelBuilder.Entity("PaymentMS.Models.OrderPaymentModel", b =>
                {
                    b.Navigation("orderPaymentCard");
                });
#pragma warning restore 612, 618
        }
    }
}