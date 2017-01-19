using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using CustomAuthWeb.Data;
using CustomAuthWeb.Models;

namespace CustomAuthWeb.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170119194450_AddBasicUsers")]
    partial class AddBasicUsers
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("CustomAuthWeb.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(60);

                    b.Property<string>("Password")
                        .IsRequired();

                    b.Property<int>("Roles");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
        }
    }
}
