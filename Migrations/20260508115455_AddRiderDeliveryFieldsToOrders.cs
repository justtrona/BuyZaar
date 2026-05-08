using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyZaar.Migrations
{
    /// <inheritdoc />
    public partial class AddRiderDeliveryFieldsToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryStatus",
                table: "Orders",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiderId",
                table: "Orders",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RiderId",
                table: "Orders",
                column: "RiderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_RiderId",
                table: "Orders",
                column: "RiderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_RiderId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RiderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RiderId",
                table: "Orders");
        }
    }
}
