using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyZaar.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerFulfillmentFieldsToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPreparing",
                table: "Orders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReadyForPickup",
                table: "Orders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparingAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyForPickupAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPreparing",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsReadyForPickup",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PreparingAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReadyForPickupAt",
                table: "Orders");
        }
    }
}
