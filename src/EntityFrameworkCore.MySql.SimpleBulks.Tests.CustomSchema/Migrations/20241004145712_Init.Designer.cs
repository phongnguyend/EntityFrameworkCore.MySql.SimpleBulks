﻿// <auto-generated />
using System;
using EntityFrameworkCore.MySql.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EntityFrameworkCore.MySql.SimpleBulks.Tests.CustomSchema.Migrations
{
    [DbContext(typeof(TestDbContext))]
    [Migration("20241004145712_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.CompositeKeyRow<int, int>", b =>
                {
                    b.Property<int>("Id1")
                        .HasColumnType("int");

                    b.Property<int>("Id2")
                        .HasColumnType("int");

                    b.Property<int>("Column1")
                        .HasColumnType("int");

                    b.Property<string>("Column2")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Column3")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id1", "Id2");

                    b.ToTable("CompositeKeyRows", "test");
                });

            modelBuilder.Entity("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.ConfigurationEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("Id1");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsSensitive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("Key1");

                    b.Property<DateTime>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp(6)");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ConfigurationEntries", "test");
                });

            modelBuilder.Entity("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.Contact", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("CountryIsoCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("char(36)");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Contacts", "test");
                });

            modelBuilder.Entity("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("CurrentCountryIsoCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Customers", "test");
                });

            modelBuilder.Entity("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.SingleKeyRow<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<Guid?>("BulkId")
                        .HasColumnType("char(36)");

                    b.Property<int?>("BulkIndex")
                        .HasColumnType("int");

                    b.Property<int>("Column1")
                        .HasColumnType("int");

                    b.Property<string>("Column2")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Column3")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("SingleKeyRows", "test");
                });

            modelBuilder.Entity("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.Contact", b =>
                {
                    b.HasOne("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.Customer", "Customer")
                        .WithMany("Contacts")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("EntityFrameworkCore.MySql.SimpleBulks.Tests.Database.Customer", b =>
                {
                    b.Navigation("Contacts");
                });
#pragma warning restore 612, 618
        }
    }
}