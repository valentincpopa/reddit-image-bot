﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RedditImageBot.Database;

#nullable disable

namespace RedditImageBot.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230716160648_Remove_Post_Constraint_NOT_NULL_ExternalId")]
    partial class Remove_Post_Constraint_NOT_NULL_ExternalId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RedditImageBot.Database.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("varchar(2048)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TimestampTz");

                    b.Property<string>("ExternalId")
                        .IsRequired()
                        .HasColumnType("varchar(16)");

                    b.Property<string>("ExternalParentId")
                        .HasColumnType("varchar(16)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("TimestampTz");

                    b.Property<int?>("PostId")
                        .HasColumnType("integer");

                    b.Property<short?>("ProcessingCount")
                        .HasColumnType("smallint");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.Property<short>("Type")
                        .HasColumnType("smallint");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("messages", (string)null);
                });

            modelBuilder.Entity("RedditImageBot.Database.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TimestampTz");

                    b.Property<string>("ExternalId")
                        .HasColumnType("varchar(16)");

                    b.Property<string>("GeneratedImageUrl")
                        .HasColumnType("varchar(1024)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("TimestampTz");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.ToTable("posts", (string)null);
                });

            modelBuilder.Entity("RedditImageBot.Database.Message", b =>
                {
                    b.HasOne("RedditImageBot.Database.Post", "Post")
                        .WithMany("Messages")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Post");
                });

            modelBuilder.Entity("RedditImageBot.Database.Post", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}