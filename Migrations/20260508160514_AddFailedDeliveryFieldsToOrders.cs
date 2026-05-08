using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyZaar.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedDeliveryFieldsToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FailedDeliveryAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailedDeliveryReason",
                table: "Orders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnToSellerAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedDeliveryAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FailedDeliveryReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnToSellerAt",
                table: "Orders");
        }
    }
}
