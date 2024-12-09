﻿using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class BookPrinter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "book_printer_entry",
                columns: table => new
                {
                    book_printer_entry_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    stamp_state = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_printer_entry", x => x.book_printer_entry_id);
                });

            migrationBuilder.CreateTable(
                name: "stamped_data",
                columns: table => new
                {
                    stamped_data_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    book_printer_entry_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stamped_data", x => x.stamped_data_id);
                    table.ForeignKey(
                        name: "FK_stamped_data_book_printer_entry_book_printer_entry_id",
                        column: x => x.book_printer_entry_id,
                        principalTable: "book_printer_entry",
                        principalColumn: "book_printer_entry_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_book_printer_entry_book_printer_entry_id",
                table: "book_printer_entry",
                column: "book_printer_entry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stamped_data_book_printer_entry_id",
                table: "stamped_data",
                column: "book_printer_entry_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stamped_data");

            migrationBuilder.DropTable(
                name: "book_printer_entry");
        }
    }
}