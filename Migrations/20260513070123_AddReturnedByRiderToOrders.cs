using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyZaar.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnedByRiderToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedAt",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnedByRiderId",
                table: "Orders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReturnedByRiderId1",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReturnedByRiderId1",
                table: "Orders",
                column: "ReturnedByRiderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RiderProfiles_ReturnedByRiderId1",
                table: "Orders",
                column: "ReturnedByRiderId1",
                principalTable: "RiderProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RiderProfiles_ReturnedByRiderId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ReturnedByRiderId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnedByRiderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnedByRiderId1",
                table: "Orders");
        }
    }
}
