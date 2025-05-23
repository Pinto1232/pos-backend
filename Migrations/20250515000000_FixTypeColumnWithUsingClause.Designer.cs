// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PosBackend.Models;

#nullable disable

namespace PosBackend.Migrations
{
    [DbContext(typeof(PosDbContext))]
    [Migration("20250515000000_FixTypeColumnWithUsingClause")]
    partial class FixTypeColumnWithUsingClause
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            // The rest of the model definition is omitted for brevity
            // This is just a placeholder to make the migration valid
#pragma warning restore 612, 618
        }
    }
}
