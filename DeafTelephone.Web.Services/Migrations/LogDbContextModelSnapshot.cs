﻿// <auto-generated />
using System;
using DeafTelephone.Web.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DeafTelephone.Web.Services.Migrations
{
    [DbContext(typeof(LogDbContext))]
    partial class LogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("DeafTelephone.Web.Core.Domain.LogRecord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ErrorTitle")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("LogLevel")
                        .HasColumnType("integer");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<long?>("OwnerScopeId")
                        .HasColumnType("bigint");

                    b.Property<long?>("RootScopeId")
                        .HasColumnType("bigint");

                    b.Property<string>("StackTrace")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerScopeId");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("DeafTelephone.Web.Core.Domain.LogScopeRecord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long?>("OwnerScopeId")
                        .HasColumnType("bigint");

                    b.Property<long?>("RootScopeId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("LogScopes");
                });

            modelBuilder.Entity("DeafTelephone.Web.Core.Domain.LogRecord", b =>
                {
                    b.HasOne("DeafTelephone.Web.Core.Domain.LogScopeRecord", "OwnerScope")
                        .WithMany("InnerLogsCollection")
                        .HasForeignKey("OwnerScopeId");

                    b.Navigation("OwnerScope");
                });

            modelBuilder.Entity("DeafTelephone.Web.Core.Domain.LogScopeRecord", b =>
                {
                    b.Navigation("InnerLogsCollection");
                });
#pragma warning restore 612, 618
        }
    }
}
