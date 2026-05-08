using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyZaar.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCancellationRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationAdminNote",
                table: "Orders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Orders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CancellationRequestStatus",
                table: "Orders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationRequestedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationReviewedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationAdminNote",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancellationRequestStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancellationRequestedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancellationReviewedAt",
                table: "Orders");
        }
    }
}
